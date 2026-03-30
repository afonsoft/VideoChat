using FamilyChat.Application.Contracts.DTOs;

namespace FamilyChat.Application.Contracts.Services;

public interface IChatAppService
{
    Task<FamilyChatGroupDto> CreateGroupAsync(CreateChatGroupDto input);
    Task<FamilyChatGroupDto> UpdateGroupAsync(Guid groupId, UpdateChatGroupDto input);
    Task DeleteGroupAsync(Guid groupId);
    Task<FamilyChatGroupDto> GetGroupAsync(Guid groupId);
    Task<List<FamilyChatGroupDto>> GetUserGroupsAsync(Guid userId);
    Task<FamilyChatGroupDto> JoinGroupAsync(JoinGroupDto input);
    Task LeaveGroupAsync(LeaveGroupDto input);
    Task<List<ChatGroupMemberDto>> GetGroupMembersAsync(Guid groupId);
}

public interface IChatMessageAppService
{
    Task<FamilyChatMessageDto> SendMessageAsync(SendMessageDto input);
    Task<FamilyChatMessageDto> EditMessageAsync(Guid messageId, EditMessageDto input);
    Task DeleteMessageAsync(Guid messageId);
    Task<MessagePagedResultDto> GetMessagesAsync(GetMessagesDto input);
    Task<FamilyChatMessageDto> GetMessageAsync(Guid messageId);
}

public interface IVideoCallAppService
{
    Task<CallInfoDto> JoinCallAsync(JoinCallDto input);
    Task LeaveCallAsync(LeaveCallDto input);
    Task<CallInfoDto> GetCallInfoAsync(Guid groupId);
    Task UpdateParticipantStatusAsync(UpdateParticipantStatusDto input);
    Task<List<CallParticipantDto>> GetCallParticipantsAsync(Guid groupId);
}
