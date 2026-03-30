using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using FamilyChat.Application.Contracts.DTOs;
using FamilyChat.Application.Contracts.Services;
using FamilyChat.HttpApi.Services;

namespace FamilyChat.HttpApi.Hubs;

public class CommunicationHub : Hub
{
    private readonly ILogger<CommunicationHub> _logger;
    private readonly IConnectionManager _connectionManager;

    public CommunicationHub(
        ILogger<CommunicationHub> logger,
        IConnectionManager connectionManager)
    {
        _logger = logger;
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

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value ??
                         Context.User?.FindFirst("user_id")?.Value ??
                         Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
