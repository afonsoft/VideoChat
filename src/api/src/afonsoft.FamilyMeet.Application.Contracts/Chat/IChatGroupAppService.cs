using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Chat;

public interface IChatGroupAppService : ICrudAppService<
    ChatGroupDto,
    Guid,
    ChatGroupListDto,
    CreateChatGroupDto,
    UpdateChatGroupDto>
{
    Task<ChatGroupDto> ActivateAsync(Guid id);
    Task<ChatGroupDto> DeactivateAsync(Guid id);
}
