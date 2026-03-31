using FamilyMeet.Application.Contracts.DTOs;

namespace FamilyMeet.Application.Contracts.Services;

public interface IChatAppService
{
    Task<FamilyMeetGroupDto> CreateGroupAsync(CreateChatGroupDto input);
    Task<FamilyMeetGroupDto> UpdateGroupAsync(Guid groupId, UpdateChatGroupDto input);
    Task DeleteGroupAsync(Guid groupId);
    Task<FamilyMeetGroupDto> GetGroupAsync(Guid groupId);
    Task<List<FamilyMeetGroupDto>> GetUserGroupsAsync(Guid userId);
    Task<FamilyMeetGroupDto> JoinGroupAsync(JoinGroupDto input);
    Task LeaveGroupAsync(LeaveGroupDto input);
    Task<List<ChatGroupMemberDto>> GetGroupMembersAsync(Guid groupId);
}

public interface IChatMessageAppService
{
    Task<FamilyMeetMessageDto> SendMessageAsync(SendMessageDto input);
    Task<FamilyMeetMessageDto> EditMessageAsync(Guid messageId, EditMessageDto input);
    Task DeleteMessageAsync(Guid messageId);
    Task<MessagePagedResultDto> GetMessagesAsync(GetMessagesDto input);
    Task<FamilyMeetMessageDto> GetMessageAsync(Guid messageId);
}

public interface IVideoCallAppService
{
    Task<CallInfoDto> JoinCallAsync(JoinCallDto input);
    Task LeaveCallAsync(LeaveCallDto input);
    Task<CallInfoDto> GetCallInfoAsync(Guid groupId);
    Task UpdateParticipantStatusAsync(UpdateParticipantStatusDto input);
    Task<List<CallParticipantDto>> GetCallParticipantsAsync(Guid groupId);
}
