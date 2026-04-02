using System;
using Volo.Abp.Domain.Entities.Auditing;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Chat;

public class ChatMessage : AuditedEntity<Guid>
{
    public Guid ChatGroupId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    protected ChatMessage()
    {
    }

    public ChatMessage(
        Guid id,
        Guid chatGroupId,
        Guid senderId,
        string senderName,
        string content,
        MessageType type = MessageType.Text,
        Guid? replyToMessageId = null
    ) : base(id)
    {
        ChatGroupId = chatGroupId;
        SenderId = senderId;
        SenderName = senderName;
        Content = content;
        Type = type;
        ReplyToMessageId = replyToMessageId;
        IsEdited = false;
        IsDeleted = false;
    }

    public void EditContent(string newContent)
    {
        Content = newContent;
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
