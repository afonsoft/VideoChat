using FamilyChat.Domain.Shared.Enums;

namespace FamilyChat.Application.Contracts.DTOs;

// Video Call DTOs
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
