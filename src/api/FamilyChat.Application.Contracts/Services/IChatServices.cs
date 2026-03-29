using FamilyChat.Application.Contracts.DTOs;

namespace FamilyChat.Application.Contracts.Services;

public interface IChatAppService
{
    Task<ChatGroupDto> CreateGroupAsync(CreateChatGroupDto input);
    Task<ChatGroupDto> UpdateGroupAsync(Guid groupId, UpdateChatGroupDto input);
    Task DeleteGroupAsync(Guid groupId);
    Task<ChatGroupDto> GetGroupAsync(Guid groupId);
    Task<List<ChatGroupDto>> GetUserGroupsAsync(Guid userId);
    Task<ChatGroupDto> JoinGroupAsync(JoinGroupDto input);
    Task LeaveGroupAsync(LeaveGroupDto input);
    Task<List<ChatGroupMemberDto>> GetGroupMembersAsync(Guid groupId);
}

public interface IChatMessageAppService
{
    Task<ChatMessageDto> SendMessageAsync(SendMessageDto message);
    Task<ChatMessageDto> EditMessageAsync(Guid messageId, EditMessageDto editDto);
    Task DeleteMessageAsync(Guid messageId);
}

public interface IVideoCallAppService
{
    Task<object> JoinCallAsync(JoinCallDto joinCallDto);
    Task LeaveCallAsync(LeaveCallDto leaveCallDto);
    Task UpdateParticipantStatusAsync(UpdateParticipantStatusDto statusDto);
}

public interface IConnectionManager
{
    Task AddConnectionAsync(Guid userId, string connectionId);
    Task RemoveConnectionAsync(Guid userId, string connectionId);
    Task<List<string>> GetConnectionsAsync(Guid userId);
}
