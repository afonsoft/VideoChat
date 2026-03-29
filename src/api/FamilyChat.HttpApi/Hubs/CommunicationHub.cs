using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using FamilyChat.Application.Contracts.DTOs;
using FamilyChat.Application.Contracts.Services;
using FamilyChat.Domain.Shared.ValueObjects;

namespace FamilyChat.HttpApi.Hubs;

public class CommunicationHub : Hub
{
    private readonly ILogger<CommunicationHub> _logger;
    private readonly IChatAppService _chatAppService;
    private readonly IChatMessageAppService _messageAppService;
    private readonly IVideoCallAppService _videoCallAppService;
    private readonly IConnectionManager _connectionManager;

    public CommunicationHub(
        ILogger<CommunicationHub> logger,
        IChatAppService chatAppService,
        IChatMessageAppService messageAppService,
        IVideoCallAppService videoCallAppService,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _chatAppService = chatAppService;
        _messageAppService = messageAppService;
        _videoCallAppService = videoCallAppService;
        _connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Connection attempt without user ID");
            Context.Abort();
            return;
        }

        await _connectionManager.AddConnectionAsync(userId, Context.ConnectionId);
        _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await _connectionManager.RemoveConnectionAsync(userId, Context.ConnectionId);

        _logger.LogInformation("User {UserId} disconnected", userId);

        await base.OnDisconnectedAsync(exception);
    }

    #region Group Management

    public async Task JoinGroup(string groupId)
    {
        var userId = GetUserId();
        var groupGuid = Guid.Parse(groupId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");

        var joinDto = new JoinGroupDto
        {
            GroupId = groupGuid,
            UserId = userId,
            UserName = Context.User?.Identity?.Name ?? "Unknown"
        };

        try
        {
            var group = await _chatAppService.JoinGroupAsync(joinDto);
            await Clients.Group($"group_{groupId}").SendAsync("UserJoinedGroup", new { UserId = userId, Group = group });

            _logger.LogInformation("User {UserId} joined group {GroupId}", userId, groupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group {GroupId} for user {UserId}", groupId, userId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join group" });
        }
    }

    public async Task LeaveGroup(string groupId)
    {
        var userId = GetUserId();
        var groupGuid = Guid.Parse(groupId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");

        var leaveDto = new LeaveGroupDto
        {
            GroupId = groupGuid,
            UserId = userId
        };

        try
        {
            await _chatAppService.LeaveGroupAsync(leaveDto);
            await Clients.Group($"group_{groupId}").SendAsync("UserLeftGroup", new { UserId = userId });

            _logger.LogInformation("User {UserId} left group {GroupId}", userId, groupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupId} for user {UserId}", groupId, userId);
        }
    }

    #endregion

    #region Chat Messages

    public async Task SendMessage(SendMessageDto message)
    {
        var userId = GetUserId();
        message.SenderId = userId;

        try
        {
            var sentMessage = await _messageAppService.SendMessageAsync(message);
            await Clients.Group($"group_{message.ChatGroupId}").SendAsync("MessageReceived", sentMessage);

            _logger.LogInformation("Message sent in group {GroupId} by user {UserId}", message.ChatGroupId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in group {GroupId}", message.ChatGroupId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to send message" });
        }
    }

    public async Task EditMessage(Guid messageId, string content)
    {
        var userId = GetUserId();

        try
        {
            var editDto = new EditMessageDto { Content = content };
            var editedMessage = await _messageAppService.EditMessageAsync(messageId, editDto);
            await Clients.Group($"group_{editedMessage.ChatGroupId}").SendAsync("MessageEdited", editedMessage);

            _logger.LogInformation("Message {MessageId} edited by user {UserId}", messageId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing message {MessageId}", messageId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to edit message" });
        }
    }

    public async Task DeleteMessage(Guid messageId)
    {
        var userId = GetUserId();

        try
        {
            await _messageAppService.DeleteMessageAsync(messageId);
            await Clients.Group($"group_{messageId}").SendAsync("MessageDeleted", new { MessageId = messageId });

            _logger.LogInformation("Message {MessageId} deleted by user {UserId}", messageId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to delete message" });
        }
    }

    #endregion

    #region Video Call & WebRTC

    public async Task JoinCall(JoinCallDto joinCallDto)
    {
        var userId = GetUserId();
        joinCallDto.UserId = userId;

        try
        {
            var callInfo = await _videoCallAppService.JoinCallAsync(joinCallDto);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"call_{joinCallDto.GroupId}");
            await Clients.Group($"call_{joinCallDto.GroupId}").SendAsync("UserJoinedCall", callInfo);
            await Clients.Caller.SendAsync("CallJoined", callInfo);

            _logger.LogInformation("User {UserId} joined call in group {GroupId}", userId, joinCallDto.GroupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining call in group {GroupId}", joinCallDto.GroupId);
            await Clients.Caller.SendAsync("Error", new { Message = "Failed to join call" });
        }
    }

    public async Task LeaveCall(LeaveCallDto leaveCallDto)
    {
        var userId = GetUserId();
        leaveCallDto.UserId = userId;

        try
        {
            await _videoCallAppService.LeaveCallAsync(leaveCallDto);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"call_{leaveCallDto.GroupId}");
            await Clients.Group($"call_{leaveCallDto.GroupId}").SendAsync("UserLeftCall", new { UserId = userId, GroupId = leaveCallDto.GroupId });

            _logger.LogInformation("User {UserId} left call in group {GroupId}", userId, leaveCallDto.GroupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving call in group {GroupId}", leaveCallDto.GroupId);
        }
    }

    public async Task UpdateParticipantStatus(UpdateParticipantStatusDto statusDto)
    {
        var userId = GetUserId();
        statusDto.UserId = userId;

        try
        {
            await _videoCallAppService.UpdateParticipantStatusAsync(statusDto);
            await Clients.Group($"call_{statusDto.GroupId}").SendAsync("ParticipantStatusUpdated", statusDto);

            _logger.LogInformation("User {UserId} updated status in group {GroupId}", userId, statusDto.GroupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant status in group {GroupId}", statusDto.GroupId);
        }
    }

    #region WebRTC Signaling

    public async Task SendWebRTCSignal(WebRTCSignalDto signal)
    {
        var userId = GetUserId();
        signal.FromUserId = userId;

        try
        {
            var targetConnections = await _connectionManager.GetConnectionsAsync(signal.ToUserId);

            foreach (var connectionId in targetConnections)
            {
                await Clients.Client(connectionId).SendAsync("WebRTCSignalReceived", signal);
            }

            _logger.LogDebug("WebRTC signal sent from {FromUserId} to {ToUserId} in room {RoomId}",
                signal.FromUserId, signal.ToUserId, signal.RoomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WebRTC signal from {FromUserId} to {ToUserId}",
                signal.FromUserId, signal.ToUserId);
        }
    }

    public async Task SendOffer(string roomId, string targetUserId, string sdp)
    {
        var userId = GetUserId();
        var signal = new WebRTCSignalDto
        {
            Type = "offer",
            Sdp = sdp,
            FromUserId = userId,
            ToUserId = Guid.Parse(targetUserId),
            RoomId = roomId
        };

        await SendWebRTCSignal(signal);
    }

    public async Task SendAnswer(string roomId, string targetUserId, string sdp)
    {
        var userId = GetUserId();
        var signal = new WebRTCSignalDto
        {
            Type = "answer",
            Sdp = sdp,
            FromUserId = userId,
            ToUserId = Guid.Parse(targetUserId),
            RoomId = roomId
        };

        await SendWebRTCSignal(signal);
    }

    public async Task SendIceCandidate(string roomId, string targetUserId, string candidate, string sdpMid, int sdpMLineIndex)
    {
        var userId = GetUserId();
        var signal = new WebRTCSignalDto
        {
            Type = "ice-candidate",
            Candidate = candidate,
            SdpMid = sdpMid,
            SdpMLineIndex = sdpMLineIndex,
            FromUserId = userId,
            ToUserId = Guid.Parse(targetUserId),
            RoomId = roomId
        };

        await SendWebRTCSignal(signal);
    }

    #endregion

    #endregion

    #region Utility Methods

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value ??
                         Context.User?.FindFirst("user_id")?.Value ??
                         Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #endregion
}
