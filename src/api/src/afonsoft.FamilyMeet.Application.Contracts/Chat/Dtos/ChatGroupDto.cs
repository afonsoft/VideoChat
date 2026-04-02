using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Chat.Dtos;

public class ChatGroupDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxParticipants { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int ParticipantCount { get; set; }
    public int MessageCount { get; set; }
}

public class CreateChatGroupDto
{
    [Required]
    [StringLength(ChatConstants.ChatGroup.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ChatConstants.ChatGroup.MaxDescriptionLength)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;

    [Range(ChatConstants.ChatGroup.DefaultMaxParticipants, ChatConstants.ChatGroup.MaxMaxParticipants)]
    public int MaxParticipants { get; set; } = ChatConstants.ChatGroup.DefaultMaxParticipants;
}

public class UpdateChatGroupDto
{
    [Required]
    [StringLength(ChatConstants.ChatGroup.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ChatConstants.ChatGroup.MaxDescriptionLength)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    [Range(ChatConstants.ChatGroup.DefaultMaxParticipants, ChatConstants.ChatGroup.MaxMaxParticipants)]
    public int MaxParticipants { get; set; }
}

public class ChatGroupListDto : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public bool? IsPublic { get; set; }
}
