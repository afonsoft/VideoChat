using SimpleConnect.Domain.Shared.ValueObjects;
using SimpleConnect.Domain.Shared.Enums;

namespace SimpleConnect.Application.Contracts.DTOs;

public class WebRTCSignalDto
{
    public string Type { get; set; } = string.Empty; // offer, answer, ice-candidate
    public string Sdp { get; set; } = string.Empty;
    public string Candidate { get; set; } = string.Empty;
    public string SdpMid { get; set; } = string.Empty;
    public int? SdpMLineIndex { get; set; }
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public string RoomId { get; set; } = string.Empty;
}

public class JoinCallDto
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool HasAudio { get; set; } = true;
    public bool HasVideo { get; set; } = true;
}

public class LeaveCallDto
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateParticipantStatusDto
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantStatus Status { get; set; }
    public bool HasAudio { get; set; }
    public bool HasVideo { get; set; }
}

public class CallInfoDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<CallParticipantDto> Participants { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime StartedAt { get; set; }
    public int MaxParticipants { get; set; }
}
