namespace FamilyMeet.Domain.Entities;

public class ChatGroupMember
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public Guid ChatGroupId { get; private set; }
    public bool IsCreator { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LastSeenAt { get; private set; }
    public bool IsActive { get; private set; }

    public ChatGroup ChatGroup { get; private set; } = null!;

    public ChatGroupMember()
    {
        Id = Guid.NewGuid();
        JoinedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public ChatGroupMember(Guid userId, string userName, bool isCreator = false)
        : this()
    {
        UserId = userId;
        UserName = userName;
        IsCreator = isCreator;
    }

    public void UpdateLastSeen()
    {
        LastSeenAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastSeenAt = DateTime.UtcNow;
    }
}
