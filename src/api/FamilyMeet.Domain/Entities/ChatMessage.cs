using System.ComponentModel.DataAnnotations;
using FamilyMeet.Domain.Shared.Constants;
using FamilyMeet.Domain.Shared.Enums;

namespace FamilyMeet.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; private set; }

    [Required]
    [MaxLength(FamilyMeetConsts.MaxMessageContentLength)]
    public string Content { get; private set; } = string.Empty;

    public Guid SenderId { get; private set; }
    public string SenderName { get; private set; } = string.Empty;

    public Guid ChatGroupId { get; private set; }
    public ChatGroup ChatGroup { get; private set; } = null!;

    public MessageType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public Guid? ReplyToMessageId { get; private set; }
    public ChatMessage? ReplyToMessage { get; private set; }

    private readonly List<ChatMessageAttachment> _attachments = new();
    public IReadOnlyCollection<ChatMessageAttachment> Attachments => _attachments.AsReadOnly();

    public ChatMessage()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Type = MessageType.Text;
        IsDeleted = false;
    }

    public ChatMessage(string content, Guid senderId, string senderName, Guid chatGroupId, MessageType type = MessageType.Text)
        : this()
    {
        Content = content;
        SenderId = senderId;
        SenderName = senderName;
        ChatGroupId = chatGroupId;
        Type = type;
    }

    public void EditContent(string newContent)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot edit a deleted message");

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Message content cannot be empty", nameof(newContent));

        if (newContent.Length > FamilyMeetConsts.MaxMessageContentLength)
            throw new ArgumentException($"Message content cannot exceed {FamilyMeetConsts.MaxMessageContentLength} characters", nameof(newContent));

        Content = newContent;
        EditedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Content = "[Message deleted]";
    }

    public void SetReply(Guid replyToMessageId)
    {
        ReplyToMessageId = replyToMessageId;
    }

    public void AddAttachment(string fileName, string fileUrl, long fileSize, string mimeType)
    {
        var attachment = new ChatMessageAttachment(fileName, fileUrl, fileSize, mimeType);
        _attachments.Add(attachment);
    }

    public bool CanBeEditedBy(Guid userId)
    {
        return !IsDeleted && SenderId == userId;
    }

    public bool CanBeDeletedBy(Guid userId, bool isGroupCreator)
    {
        return !IsDeleted && (SenderId == userId || isGroupCreator);
    }
}
