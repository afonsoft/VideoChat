using System.ComponentModel.DataAnnotations;
using FamilyChat.Domain.Shared.Constants;
using FamilyChat.Domain.Shared.Enums;

namespace FamilyChat.Application.Contracts.DTOs;

public class FamilyChatMessageDto
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(FamilyChatConsts.MaxMessageContentLength)]
    public string Content { get; set; } = string.Empty;

    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid ChatGroupId { get; set; }
    public MessageType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public FamilyChatMessageDto? ReplyToMessage { get; set; }

    public List<ChatMessageAttachmentDto> Attachments { get; set; } = new();
}

public class ChatMessageAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class SendMessageDto
{
    [Required]
    [MaxLength(FamilyChatConsts.MaxMessageContentLength)]
    public string Content { get; set; } = string.Empty;

    public Guid ChatGroupId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public Guid? ReplyToMessageId { get; set; }
    public Guid SenderId { get; set; }
}

public class EditMessageDto
{
    [Required]
    [MaxLength(FamilyChatConsts.MaxMessageContentLength)]
    public string Content { get; set; } = string.Empty;
}

public class GetMessagesDto
{
    public Guid ChatGroupId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public DateTime? BeforeDate { get; set; }
    public DateTime? AfterDate { get; set; }
}

public class MessagePagedResultDto
{
    public List<FamilyChatMessageDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
}