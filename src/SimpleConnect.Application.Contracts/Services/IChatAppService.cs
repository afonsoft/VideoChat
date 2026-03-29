using SimpleConnect.Application.Contracts.DTOs;

namespace SimpleConnect.Application.Contracts.Services;

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
    Task<ChatMessageDto> SendMessageAsync(SendMessageDto input);
    Task<ChatMessageDto> EditMessageAsync(Guid messageId, EditMessageDto input);
    Task DeleteMessageAsync(Guid messageId);
    Task<MessagePagedResultDto> GetMessagesAsync(GetMessagesDto input);
    Task<ChatMessageDto> GetMessageAsync(Guid messageId);
}

public interface IVideoCallAppService
{
    Task<CallInfoDto> JoinCallAsync(JoinCallDto input);
    Task LeaveCallAsync(LeaveCallDto input);
    Task<CallInfoDto> GetCallInfoAsync(Guid groupId);
    Task UpdateParticipantStatusAsync(UpdateParticipantStatusDto input);
    Task<List<CallParticipantDto>> GetCallParticipantsAsync(Guid groupId);
}
