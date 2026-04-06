using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using afonsoft.FamilyMeet.Chat.Dtos;
using afonsoft.FamilyMeet.Localization;
using Microsoft.Extensions.Logging;

namespace afonsoft.FamilyMeet.Chat.Hubs;

[Authorize]
public class ChatHub : AbpHub
{
    private readonly ILogger<ChatHub> _logger;

    // Armazena informações de conexão para P2P
    private static readonly ConcurrentDictionary<string, UserConnectionInfo> _userConnections = new();
    private static readonly ConcurrentDictionary<Guid, List<string>> _groupConnections = new();

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    #region Chat Message Operations

    public async Task SendMessageAsync(ChatMessageSignalRDto message)
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) sent message to group {GroupId}", userId, userName, message.ChatGroupId);

        // Broadcast to all users in the group
        await Clients.Group($"ChatGroup_{message.ChatGroupId}").SendAsync("ReceiveMessage", new
        {
            Message = message,
            Timestamp = DateTime.UtcNow,
            SenderId = userId,
            SenderName = userName
        });
    }

    public async Task EditMessageAsync(Guid messageId, string newContent)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) edited message {MessageId}", userId, userName, messageId);

        // Broadcast to all groups the user is in
        var userGroups = GetUserGroups(userId);
        foreach (var groupId in userGroups)
        {
            await Clients.Group($"ChatGroup_{groupId}").SendAsync("MessageEdited", new
            {
                MessageId = messageId,
                NewContent = newContent,
                EditedBy = userName,
                EditedAt = DateTime.UtcNow
            });
        }
    }

    public async Task DeleteMessageAsync(Guid messageId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) deleted message {MessageId}", userId, userName, messageId);

        // Broadcast to all groups the user is in
        var userGroups = GetUserGroups(userId);
        foreach (var groupId in userGroups)
        {
            await Clients.Group($"ChatGroup_{groupId}").SendAsync("MessageDeleted", new
            {
                MessageId = messageId,
                DeletedBy = userName,
                DeletedAt = DateTime.UtcNow
            });
        }
    }

    #endregion

    #region Group Management

    public async Task JoinGroupAsync(Guid groupId)
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        await Groups.AddToGroupAsync(connectionId, $"ChatGroup_{groupId}");

        // Track group connections
        _groupConnections.AddOrUpdate(groupId,
            new List<string> { connectionId },
            (key, existing) => { existing.Add(connectionId); return existing; });

        _logger.LogInformation("User {UserId} ({UserName}) joined group {GroupId}", userId, userName, groupId);

        // Notify others in the group
        await Clients.OthersInGroup($"ChatGroup_{groupId}").SendAsync("UserJoined", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName,
            ConnectionId = connectionId,
            JoinedAt = DateTime.UtcNow
        });

        // Send current group status
        await SendGroupStatusAsync(groupId);
    }

    public async Task LeaveGroupAsync(Guid groupId)
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        await Groups.RemoveFromGroupAsync(connectionId, $"ChatGroup_{groupId}");

        // Remove from group connections
        if (_groupConnections.TryGetValue(groupId, out var connections))
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _groupConnections.TryRemove(groupId, out _);
            }
        }

        _logger.LogInformation("User {UserId} ({UserName}) left group {GroupId}", userId, userName, groupId);

        // Notify others in the group
        await Clients.OthersInGroup($"ChatGroup_{groupId}").SendAsync("UserLeft", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName,
            LeftAt = DateTime.UtcNow
        });

        // Send updated group status
        await SendGroupStatusAsync(groupId);
    }

    #endregion

    #region User Status & Typing

    public async Task UserTypingAsync(Guid groupId, bool isTyping)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        await Clients.OthersInGroup($"ChatGroup_{groupId}").SendAsync("UserTyping", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task UpdateOnlineStatusAsync(bool isOnline)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        // Update user connection info
        if (_userConnections.TryGetValue(Context.ConnectionId, out var userInfo))
        {
            userInfo.IsOnline = isOnline;
            userInfo.LastSeen = DateTime.UtcNow;
        }

        // Broadcast to all groups the user is in
        var userGroups = GetUserGroups(userId);
        foreach (var groupId in userGroups)
        {
            await Clients.Group($"ChatGroup_{groupId}").SendAsync("UserStatusChanged", new
            {
                UserId = userId,
                UserName = userName,
                IsOnline = isOnline,
                LastSeen = DateTime.UtcNow
            });
        }
    }

    #endregion

    #region Video Call P2P Support

    public async Task StartVideoCallAsync(Guid groupId, Guid targetUserId)
    {
        var callerId = CurrentUser.Id ?? Guid.Empty;
        var callerName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {CallerId} ({CallerName}) starting video call with {TargetUserId} in group {GroupId}",
            callerId, callerName, targetUserId, groupId);

        // Find target user connection
        var targetConnection = _userConnections.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);
        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("VideoCallRequest", new
            {
                GroupId = groupId,
                CallerId = callerId,
                CallerName = callerName,
                CallId = Guid.NewGuid(),
                StartedAt = DateTime.UtcNow
            });
        }
        else
        {
            // Target user not online
            await Clients.Caller.SendAsync("VideoCallError", new
            {
                Error = "Target user is not online",
                TargetUserId = targetUserId
            });
        }
    }

    public async Task AcceptVideoCallAsync(Guid callId, Guid callerId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) accepted video call {CallId} from {CallerId}",
            userId, userName, callId, callerId);

        // Find caller connection
        var callerConnection = _userConnections.FirstOrDefault(kvp => kvp.Value.UserId == callerId);

        if (callerConnection.Key != null)
        {
            await Clients.Client(callerConnection.Key).SendAsync("VideoCallAccepted", new
            {
                CallId = callId,
                TargetUserId = userId,
                TargetUserName = userName,
                AcceptedAt = DateTime.UtcNow
            });
        }
    }

    public async Task RejectVideoCallAsync(Guid callId, Guid callerId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) rejected video call {CallId} from {CallerId}",
            userId, userName, callId, callerId);

        // Find caller connection
        var callerConnection = _userConnections.FirstOrDefault(kvp => kvp.Value.UserId == callerId);

        if (callerConnection.Key != null)
        {
            await Clients.Client(callerConnection.Key).SendAsync("VideoCallRejected", new
            {
                CallId = callId,
                TargetUserId = userId,
                TargetUserName = userName,
                RejectedAt = DateTime.UtcNow
            });
        }
    }

    public async Task EndVideoCallAsync(Guid callId, Guid targetUserId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) ended video call {CallId} with {TargetUserId}",
            userId, userName, callId, targetUserId);

        // Notify both parties
        var targetConnection = _userConnections.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);

        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("VideoCallEnded", new
            {
                CallId = callId,
                EndedBy = userId,
                EndedByName = userName,
                EndedAt = DateTime.UtcNow
            });
        }

        await Clients.Caller.SendAsync("VideoCallEnded", new
        {
            CallId = callId,
            EndedBy = userId,
            EndedByName = userName,
            EndedAt = DateTime.UtcNow
        });
    }

    // WebRTC Signaling for P2P
    public async Task SendWebRTCSignalAsync(Guid targetUserId, object signal)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;

        _logger.LogDebug("WebRTC signal from {UserId} to {TargetUserId}", userId, targetUserId);

        // Find target user connection
        var targetConnection = _userConnections.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);

        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("WebRTCSignal", new
            {
                FromUserId = userId,
                Signal = signal,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    #endregion

    #region Connection Lifecycle

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        // Store user connection info
        _userConnections[connectionId] = new UserConnectionInfo
        {
            UserId = userId,
            UserName = userName,
            ConnectedAt = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow,
            IsOnline = true
        };

        _logger.LogInformation("User {UserId} ({UserName}) connected to chat hub", userId, userName);

        await Clients.Caller.SendAsync("Connected", new
        {
            ConnectionId = connectionId,
            UserId = userId,
            UserName = userName,
            ConnectedAt = DateTime.UtcNow
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        // Remove user connection
        if (_userConnections.TryRemove(connectionId, out var userInfo))
        {
            userInfo.IsOnline = false;
            userInfo.LastSeen = DateTime.UtcNow;
        }

        // Remove from all groups
        var userGroups = GetUserGroups(userId);
        foreach (var groupId in userGroups)
        {
            await LeaveGroupAsync(groupId);
        }

        _logger.LogInformation("User {UserId} ({UserName}) disconnected from chat hub", userId, userName);

        // Notify all groups about user offline status
        foreach (var groupId in userGroups)
        {
            await Clients.Group($"ChatGroup_{groupId}").SendAsync("UserStatusChanged", new
            {
                UserId = userId,
                UserName = userName,
                IsOnline = false,
                LastSeen = DateTime.UtcNow
            });
        }

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Helper Methods

    private async Task SendGroupStatusAsync(Guid groupId)
    {
        if (_groupConnections.TryGetValue(groupId, out var connections))
        {
            var onlineUsers = new List<object>();

            foreach (var connId in connections)
            {
                if (_userConnections.TryGetValue(connId, out var userInfo))
                {
                    onlineUsers.Add(new
                    {
                        UserId = userInfo.UserId,
                        UserName = userInfo.UserName,
                        IsOnline = userInfo.IsOnline,
                        LastSeen = userInfo.LastSeen
                    });
                }
            }

            await Clients.Group($"ChatGroup_{groupId}").SendAsync("GroupStatus", new
            {
                GroupId = groupId,
                OnlineUsers = onlineUsers,
                TotalUsers = onlineUsers.Count,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private List<Guid> GetUserGroups(Guid userId)
    {
        // This would typically query the database for user's groups
        // For now, return empty list - implement based on your data access layer
        return new List<Guid>();
    }

    #endregion
}

public class UserConnectionInfo
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsOnline { get; set; }
}
