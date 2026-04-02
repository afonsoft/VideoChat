using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Chat;

public interface IChatMessageAppService : ICrudAppService<
    ChatMessageDto,
    Guid,
    ChatMessageListDto,
    CreateChatMessageDto,
    UpdateChatMessageDto>
{
    Task<ChatMessageDto> SendAsync(CreateChatMessageDto input);
    Task<ChatMessageDto> EditAsync(Guid id, UpdateChatMessageDto input);
    Task DeleteSoftAsync(Guid id);
}

public interface IChatHub
{
    Task SendMessageAsync(ChatMessageSignalRDto message);
    Task JoinGroupAsync(Guid groupId);
    Task LeaveGroupAsync(Guid groupId);
    Task UserJoinedAsync(Guid groupId, Guid userId, string userName);
    Task UserLeftAsync(Guid groupId, Guid userId, string userName);
    Task UserTypingAsync(Guid groupId, Guid userId, string userName);
}
