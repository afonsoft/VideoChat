using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace afonsoft.FamilyMeet.Chat;

public class ChatGroup : AuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxParticipants { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastMessageAt { get; set; }

    protected ChatGroup()
    {
    }

    public ChatGroup(
        Guid id,
        string name,
        string description = "",
        bool isPublic = true,
        int maxParticipants = 100
    ) : base(id)
    {
        Name = name;
        Description = description;
        IsPublic = isPublic;
        MaxParticipants = maxParticipants;
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void SetLastMessageTime(DateTime dateTime)
    {
        LastMessageAt = dateTime;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
