using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using FamilyMeet.Application.Contracts.DTOs;
using FamilyMeet.Application.Contracts.Services;
using FamilyMeet.HttpApi.Services;

namespace FamilyMeet.HttpApi.Hubs;

public class CommunicationHub : Hub
{
    private readonly ILogger<CommunicationHub> _logger;
    private readonly IConnectionManager _connectionManager;
    private readonly IVideoCallAppService _videoCallAppService;

    public CommunicationHub(
        ILogger<CommunicationHub> logger,
        IConnectionManager connectionManager,
        IVideoCallAppService videoCallAppService)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _videoCallAppService = videoCallAppService;
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

    // WebRTC Video Call Methods
    public async Task JoinVideoCall(string groupId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        var groupGuid = Guid.Parse(groupId);
        await Groups.AddToGroupAsync(Context.ConnectionId, $"call_{groupId}");

        // Notify other participants
        await Clients.OthersInGroup($"call_{groupId}").SendAsync("ParticipantJoined", new { userId = userId });

        _logger.LogInformation("User {UserId} joined video call for group {GroupId}", userId, groupGuid);
    }

    public async Task LeaveVideoCall(string groupId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"call_{groupId}");

        // Notify other participants
        await Clients.OthersInGroup($"call_{groupId}").SendAsync("ParticipantLeft", new { userId = userId });

        _logger.LogInformation("User {UserId} left video call for group {GroupId}", userId, groupId);
    }

    public async Task SendWebRTCSignal(WebRTCSignalDto signal)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        // Set the sender
        signal.FromUserId = userId;

        // Send to specific user or room
        if (!string.IsNullOrEmpty(signal.RoomId))
        {
            await Clients.OthersInGroup($"call_{signal.RoomId}").SendAsync("WebRTCSignal", signal);
        }
        else if (signal.ToUserId != Guid.Empty)
        {
            var targetConnections = await _connectionManager.GetConnectionsAsync(signal.ToUserId);
            foreach (var connection in targetConnections)
            {
                await Clients.Client(connection).SendAsync("WebRTCSignal", signal);
            }
        }

        _logger.LogDebug("WebRTC signal sent from {FromUserId} to {ToUserId} in room {RoomId}",
            signal.FromUserId, signal.ToUserId, signal.RoomId);
    }

    public async Task ToggleAudio(string groupId, bool isMuted)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        await Clients.OthersInGroup($"call_{groupId}").SendAsync("AudioToggled", new { userId = userId, isMuted });
    }

    public async Task ToggleVideo(string groupId, bool isVideoOff)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        await Clients.OthersInGroup($"call_{groupId}").SendAsync("VideoToggled", new { userId = userId, isVideoOff });
    }

    public async Task StartScreenShare(string groupId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        await Clients.OthersInGroup($"call_{groupId}").SendAsync("ScreenShareStarted", new { userId = userId });
    }

    public async Task StopScreenShare(string groupId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        await Clients.OthersInGroup($"call_{groupId}").SendAsync("ScreenShareStopped", new { userId = userId });
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value ??
                         Context.User?.FindFirst("user_id")?.Value ??
                         Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
