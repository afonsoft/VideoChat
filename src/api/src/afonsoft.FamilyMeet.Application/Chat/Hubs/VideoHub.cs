using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace afonsoft.FamilyMeet.Chat.Hubs;

[Authorize]
public class VideoHub : AbpHub
{
    private readonly ILogger<VideoHub> _logger;

    // Armazena informações de chamadas de vídeo ativas
    private static readonly ConcurrentDictionary<Guid, VideoCallInfo> _activeCalls = new();

    private static readonly ConcurrentDictionary<string, UserVideoInfo> _userVideoSessions = new();

    public VideoHub(ILogger<VideoHub> logger)
    {
        _logger = logger;
    }

    #region Video Call Management

    public async Task StartVideoCallAsync(Guid groupId, Guid targetUserId)
    {
        var callerId = CurrentUser.Id ?? Guid.Empty;
        var callerName = CurrentUser.Name ?? "Anonymous";
        var callId = Guid.NewGuid();

        _logger.LogInformation("User {CallerId} ({CallerName}) starting video call {CallId} with {TargetUserId} in group {GroupId}",
            callerId, callerName, callId, targetUserId, groupId);

        // Verificar se ambos os usuários estão online
        var targetConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);
        var callerConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == callerId);

        if (targetConnection.Key == null)
        {
            await Clients.Caller.SendAsync("VideoCallError", new
            {
                Error = "Target user is not available for video calls",
                TargetUserId = targetUserId,
                CallId = callId
            });
            return;
        }

        // Criar informações da chamada
        var callInfo = new VideoCallInfo
        {
            CallId = callId,
            GroupId = groupId,
            CallerId = callerId,
            CallerName = callerName,
            TargetUserId = targetUserId,
            StartedAt = DateTime.UtcNow,
            Status = VideoCallStatus.Initiated
        };

        _activeCalls[callId] = callInfo;

        // Notificar o usuário alvo
        await Clients.Client(targetConnection.Key).SendAsync("IncomingVideoCall", new
        {
            CallId = callId,
            GroupId = groupId,
            CallerId = callerId,
            CallerName = callerName,
            StartedAt = DateTime.UtcNow
        });

        // Confirmar para o chamador
        await Clients.Caller.SendAsync("VideoCallInitiated", new
        {
            CallId = callId,
            TargetUserId = targetUserId,
            Status = "waiting_for_response"
        });
    }

    public async Task AcceptVideoCallAsync(Guid callId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            await Clients.Caller.SendAsync("VideoCallError", new
            {
                Error = "Call not found",
                CallId = callId
            });
            return;
        }

        // Verificar se o usuário é o alvo da chamada
        if (callInfo.TargetUserId != userId)
        {
            await Clients.Caller.SendAsync("VideoCallError", new
            {
                Error = "You are not the target of this call",
                CallId = callId
            });
            return;
        }

        callInfo.Status = VideoCallStatus.Active;
        callInfo.AcceptedAt = DateTime.UtcNow;

        _logger.LogInformation("User {UserId} ({UserName}) accepted video call {CallId}",
            userId, userName, callId);

        // Notificar o chamador
        var callerConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == callInfo.CallerId);
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

        // Iniciar troca de WebRTC signaling
        await Clients.Caller.SendAsync("StartWebRTCExchange", new
        {
            CallId = callId,
            PeerUserId = callInfo.CallerId,
            IsInitiator = false
        });

        if (callerConnection.Key != null)
        {
            await Clients.Client(callerConnection.Key).SendAsync("StartWebRTCExchange", new
            {
                CallId = callId,
                PeerUserId = userId,
                IsInitiator = true
            });
        }
    }

    public async Task RejectVideoCallAsync(Guid callId, string reason = "User declined")
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            return;
        }

        callInfo.Status = VideoCallStatus.Rejected;
        callInfo.EndedAt = DateTime.UtcNow;

        _logger.LogInformation("User {UserId} ({UserName}) rejected video call {CallId}. Reason: {Reason}",
            userId, userName, callId, reason);

        // Notificar o chamador
        var callerConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == callInfo.CallerId);
        if (callerConnection.Key != null)
        {
            await Clients.Client(callerConnection.Key).SendAsync("VideoCallRejected", new
            {
                CallId = callId,
                TargetUserId = userId,
                TargetUserName = userName,
                Reason = reason,
                RejectedAt = DateTime.UtcNow
            });
        }

        // Limpar chamada
        _activeCalls.TryRemove(callId, out _);
    }

    public async Task EndVideoCallAsync(Guid callId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            return;
        }

        callInfo.Status = VideoCallStatus.Ended;
        callInfo.EndedAt = DateTime.UtcNow;

        _logger.LogInformation("User {UserId} ({UserName}) ended video call {CallId}",
            userId, userName, callId);

        // Notificar ambos os participantes
        var callerConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == callInfo.CallerId);
        var targetConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == callInfo.TargetUserId);

        var endNotification = new
        {
            CallId = callId,
            EndedBy = userId,
            EndedByName = userName,
            EndedAt = DateTime.UtcNow,
            Duration = callInfo.EndedAt.Value - callInfo.StartedAt
        };

        if (callerConnection.Key != null)
        {
            await Clients.Client(callerConnection.Key).SendAsync("VideoCallEnded", endNotification);
        }

        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("VideoCallEnded", endNotification);
        }

        // Limpar chamada
        _activeCalls.TryRemove(callId, out _);
    }

    #endregion Video Call Management

    #region WebRTC Signaling

    public async Task SendWebRTCOfferAsync(Guid callId, object offer)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            return;
        }

        // Enviar offer para o outro participante
        var targetUserId = callInfo.CallerId == userId ? callInfo.TargetUserId : callInfo.CallerId;
        var targetConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);

        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("WebRTCOffer", new
            {
                CallId = callId,
                FromUserId = userId,
                Offer = offer,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public async Task SendWebRTCAnswerAsync(Guid callId, object answer)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            return;
        }

        // Enviar answer para o chamador
        var callerConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == callInfo.CallerId);

        if (callerConnection.Key != null)
        {
            await Clients.Client(callerConnection.Key).SendAsync("WebRTCAnswer", new
            {
                CallId = callId,
                FromUserId = userId,
                Answer = answer,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public async Task SendWebRTCIceCandidateAsync(Guid callId, object candidate)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            return;
        }

        // Enviar ICE candidate para o outro participante
        var targetUserId = callInfo.CallerId == userId ? callInfo.TargetUserId : callInfo.CallerId;
        var targetConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);

        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("WebRTCIceCandidate", new
            {
                CallId = callId,
                FromUserId = userId,
                Candidate = candidate,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    #endregion WebRTC Signaling

    #region Screen Sharing

    public async Task StartScreenShareAsync(Guid callId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            return;
        }

        _logger.LogInformation("User {UserId} started screen sharing in call {CallId}", userId, callId);

        // Notificar o outro participante
        var targetUserId = callInfo.CallerId == userId ? callInfo.TargetUserId : callInfo.CallerId;
        var targetConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);

        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("ScreenShareStarted", new
            {
                CallId = callId,
                SharingUserId = userId,
                StartedAt = DateTime.UtcNow
            });
        }
    }

    public async Task StopScreenShareAsync(Guid callId)
    {
        var userId = CurrentUser.Id ?? Guid.Empty;

        if (!_activeCalls.TryGetValue(callId, out var callInfo))
        {
            return;
        }

        _logger.LogInformation("User {UserId} stopped screen sharing in call {CallId}", userId, callId);

        // Notificar o outro participante
        var targetUserId = callInfo.CallerId == userId ? callInfo.TargetUserId : callInfo.CallerId;
        var targetConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);

        if (targetConnection.Key != null)
        {
            await Clients.Client(targetConnection.Key).SendAsync("ScreenShareStopped", new
            {
                CallId = callId,
                SharingUserId = userId,
                StoppedAt = DateTime.UtcNow
            });
        }
    }

    #endregion Screen Sharing

    #region Connection Management

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userName = CurrentUser.Name ?? "Anonymous";

        // Armazenar informações de vídeo do usuário
        _userVideoSessions[connectionId] = new UserVideoInfo
        {
            UserId = userId,
            UserName = userName,
            ConnectedAt = DateTime.UtcNow,
            IsInVideoCall = false
        };

        _logger.LogInformation("User {UserId} ({UserName}) connected to video hub", userId, userName);

        await Clients.Caller.SendAsync("VideoHubConnected", new
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

        // Encerrar chamadas ativas do usuário
        var userCalls = _activeCalls.Where(kvp =>
            kvp.Value.CallerId == userId || kvp.Value.TargetUserId == userId).ToList();

        foreach (var call in userCalls)
        {
            await EndVideoCallAsync(call.Key);
        }

        // Remover informações de vídeo
        _userVideoSessions.TryRemove(connectionId, out _);

        _logger.LogInformation("User {UserId} ({UserName}) disconnected from video hub", userId, userName);

        await base.OnDisconnectedAsync(exception);
    }

    #endregion Connection Management

    #region Helper Methods

    public async Task GetActiveCallsAsync()
    {
        var userId = CurrentUser.Id ?? Guid.Empty;
        var userCalls = _activeCalls.Values
            .Where(call => call.CallerId == userId || call.TargetUserId == userId)
            .ToList();

        await Clients.Caller.SendAsync("ActiveCalls", userCalls);
    }

    public async Task GetUserVideoStatusAsync(Guid targetUserId)
    {
        var targetConnection = _userVideoSessions.FirstOrDefault(kvp => kvp.Value.UserId == targetUserId);

        await Clients.Caller.SendAsync("UserVideoStatus", new
        {
            UserId = targetUserId,
            IsOnline = targetConnection.Key != null,
            IsInVideoCall = targetConnection.Value?.IsInVideoCall ?? false,
            LastSeen = targetConnection.Value?.ConnectedAt
        });
    }

    #endregion Helper Methods
}

#region Supporting Classes

public class VideoCallInfo
{
    public Guid CallId { get; set; }
    public Guid GroupId { get; set; }
    public Guid CallerId { get; set; }
    public string CallerName { get; set; } = string.Empty;
    public Guid TargetUserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public VideoCallStatus Status { get; set; }
}

public class UserVideoInfo
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public bool IsInVideoCall { get; set; }
    public DateTime? LastVideoCall { get; set; }
}

public enum VideoCallStatus
{
    Initiated,
    Active,
    Ended,
    Rejected,
    Failed
}

#endregion Supporting Classes