using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace afonsoft.FamilyMeet.Chat;

public class ChatParticipant : AuditedEntity<Guid>
{
    public Guid ChatGroupId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsMuted { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? BannedUntil { get; set; }
    public bool IsCreator { get; set; }

    protected ChatParticipant()
    {
    }

    public ChatParticipant(
        Guid id,
        Guid chatGroupId,
        Guid userId,
        string userName,
        bool isCreator = false
    ) : base(id)
    {
        ChatGroupId = chatGroupId;
        UserId = userId;
        UserName = userName;
        IsCreator = isCreator;
        IsOnline = true;
        IsMuted = false;
        IsBanned = false;
    }

    public void SetOnlineStatus(bool isOnline)
    {
        IsOnline = isOnline;
        if (!isOnline)
        {
            LastSeenAt = DateTime.UtcNow;
        }
    }

    public void Mute()
    {
        IsMuted = true;
    }

    public void Unmute()
    {
        IsMuted = false;
    }

    public void Ban(TimeSpan duration)
    {
        IsBanned = true;
        BannedUntil = DateTime.UtcNow.Add(duration);
    }

    public void Unban()
    {
        IsBanned = false;
        BannedUntil = null;
    }

    public void UpdateUserName(string userName)
    {
        UserName = userName;
    }
}
