using FamilyMeet.Domain.Shared.Enums;

namespace FamilyMeet.Domain.Shared.ValueObjects;

public record CallParticipant
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public ParticipantStatus Status { get; init; }
    public DateTime JoinedAt { get; init; }
    public bool HasAudio { get; init; }
    public bool HasVideo { get; init; }

    public CallParticipant(Guid userId, string userName, ParticipantStatus status = ParticipantStatus.Disconnected)
    {
        UserId = userId;
        UserName = userName;
        Status = status;
        JoinedAt = DateTime.UtcNow;
        HasAudio = false;
        HasVideo = false;
    }

    public CallParticipant WithStatus(ParticipantStatus status)
    {
        return this with { Status = status };
    }

    public CallParticipant WithAudio(bool hasAudio)
    {
        return this with { HasAudio = hasAudio };
    }

    public CallParticipant WithVideo(bool hasVideo)
    {
        return this with { HasVideo = hasVideo };
    }
}
