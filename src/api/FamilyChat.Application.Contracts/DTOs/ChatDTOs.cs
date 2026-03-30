using System.ComponentModel.DataAnnotations;
using FamilyChat.Domain.Shared.Enums;

namespace FamilyChat.Application.Contracts.DTOs;

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
    public Guid ChatGroupId { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public Guid SenderId { get; set; }
}

public class JoinGroupDto
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class LeaveGroupDto
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
}

public class CreateChatGroupDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public GroupType Type { get; set; } = GroupType.Chat;

    public Guid CreatorId { get; set; }

    public int MaxParticipants { get; set; } = 10;
}

public class UpdateChatGroupDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class ChatGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GroupType Type { get; set; }
    public Guid CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public bool IsActive { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipantsCount { get; set; }
    public List<ChatGroupMemberDto> Members { get; set; } = new();
}

public class ChatGroupMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsCreator { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public Guid ChatGroupId { get; set; }
    public MessageType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsOwn { get; set; }
}

public class EditMessageDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}
