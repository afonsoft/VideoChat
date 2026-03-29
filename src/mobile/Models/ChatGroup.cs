namespace FamilyChat.Mobile.Models;

public class ChatGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GroupType Type { get; set; }
    public Guid CreatorId { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipantsCount { get; set; }
    public int ActiveCallParticipantsCount { get; set; }
    public List<ChatGroupMember> Members { get; set; } = new();
    public List<CallParticipant> ActiveCallParticipants { get; set; } = new();
}

public class ChatGroupMember
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsCreator { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsInCall { get; set; }
}

public class CallParticipant
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool HasAudio { get; set; }
    public bool HasVideo { get; set; }
}
