using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Chat.Dtos;

public class ChatMessageDto : FullAuditedEntityDto<Guid>
{
    public Guid ChatGroupId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public Guid? ReplyToMessageId { get; set; }
    public ChatMessageDto ReplyToMessage { get; set; }
    public new bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class CreateChatMessageDto
{
    [Required]
    public Guid ChatGroupId { get; set; }

    [Required]
    [StringLength(ChatConstants.ChatMessage.MaxContentLength)]
    public string Content { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public Guid? ReplyToMessageId { get; set; }
}

public class UpdateChatMessageDto
{
    [Required]
    [StringLength(ChatConstants.ChatMessage.MaxContentLength)]
    public string Content { get; set; } = string.Empty;
}

public class ChatMessageListDto : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid ChatGroupId { get; set; }

    public MessageType? Type { get; set; }
    public Guid? SenderId { get; set; }
    public bool? IncludeDeleted { get; set; }
}
