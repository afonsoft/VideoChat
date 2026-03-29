using System.ComponentModel.DataAnnotations;
using SimpleConnect.Domain.Shared.Constants;
using SimpleConnect.Domain.Shared.Enums;

namespace SimpleConnect.Application.Contracts.DTOs;

public class ChatGroupDto
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(SimpleConnectConsts.MaxGroupNameLength)]
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

    public List<ChatGroupMemberDto> Members { get; set; } = new();
    public List<CallParticipantDto> ActiveCallParticipants { get; set; } = new();
}

public class ChatGroupMemberDto
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

public class CallParticipantDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool HasAudio { get; set; }
    public bool HasVideo { get; set; }
}

public class CreateChatGroupDto
{
    [Required]
    [MaxLength(SimpleConnectConsts.MaxGroupNameLength)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public GroupType Type { get; set; }
    public int MaxParticipants { get; set; } = SimpleConnectConsts.MaxVideoCallParticipants;
    public Guid CreatorId { get; set; }
}

public class UpdateChatGroupDto
{
    [Required]
    [MaxLength(SimpleConnectConsts.MaxGroupNameLength)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
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
