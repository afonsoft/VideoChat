using System;
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

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public async Task SendMessageAsync(ChatMessageSignalRDto message)
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) sent message to group {GroupId}", userId, userName, message.ChatGroupId);

        // Broadcast to all users in the group
        await Clients.Group($"ChatGroup_{message.ChatGroupId}").SendAsync("ReceiveMessage", message);
    }

    public async Task JoinGroupAsync(Guid groupId)
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        await Groups.AddToGroupAsync(connectionId, $"ChatGroup_{groupId}");

        _logger.LogInformation("User {UserId} ({UserName}) joined group {GroupId}", userId, userName, groupId);

        // Notify others in the group
        await Clients.OthersInGroup($"ChatGroup_{groupId}").SendAsync("UserJoined", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName
        });
    }

    public async Task LeaveGroupAsync(Guid groupId)
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        await Groups.RemoveFromGroupAsync(connectionId, $"ChatGroup_{groupId}");

        _logger.LogInformation("User {UserId} ({UserName}) left group {GroupId}", userId, userName, groupId);

        // Notify others in the group
        await Clients.OthersInGroup($"ChatGroup_{groupId}").SendAsync("UserLeft", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName
        });
    }

    public async Task UserJoinedAsync(Guid groupId, Guid userId, string userName)
    {
        await Clients.Group($"ChatGroup_{groupId}").SendAsync("UserJoined", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName
        });
    }

    public async Task UserLeftAsync(Guid groupId, Guid userId, string userName)
    {
        await Clients.Group($"ChatGroup_{groupId}").SendAsync("UserLeft", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName
        });
    }

    public async Task UserTypingAsync(Guid groupId, Guid userId, string userName)
    {
        await Clients.OthersInGroup($"ChatGroup_{groupId}").SendAsync("UserTyping", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName
        });
    }

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) connected to chat hub", userId, userName);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        _logger.LogInformation("User {UserId} ({UserName}) disconnected from chat hub", userId, userName);

        await base.OnDisconnectedAsync(exception);
    }
}
