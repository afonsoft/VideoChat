using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using afonsoft.FamilyMeet.Chat.Dtos;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Chat;

public class ChatGroupAppService : CrudAppService<
    ChatGroup,
    ChatGroupDto,
    Guid,
    ChatGroupListDto,
    CreateChatGroupDto,
    UpdateChatGroupDto>, IChatGroupAppService
{
    public ChatGroupAppService(IRepository<ChatGroup, Guid> repository) : base(repository)
    {
    }

    public override async Task<ChatGroupDto> CreateAsync(CreateChatGroupDto input)
    {
        var chatGroup = new ChatGroup(
            GuidGenerator.Create(),
            input.Name,
            input.Description,
            input.IsPublic,
            input.MaxParticipants
        );

        await Repository.InsertAsync(chatGroup, autoSave: true);

        return await MapToGetOutputDtoAsync(chatGroup);
    }

    public override async Task<ChatGroupDto> UpdateAsync(Guid id, UpdateChatGroupDto input)
    {
        var chatGroup = await Repository.GetAsync(id);
        chatGroup.UpdateName(input.Name);
        chatGroup.UpdateDescription(input.Description);
        chatGroup.IsPublic = input.IsPublic;
        chatGroup.MaxParticipants = input.MaxParticipants;

        await Repository.UpdateAsync(chatGroup, autoSave: true);

        return await MapToGetOutputDtoAsync(chatGroup);
    }

    public override async Task<PagedResultDto<ChatGroupDto>> GetListAsync(ChatGroupListDto input)
    {
        var queryable = await Repository.GetQueryableAsync();

        queryable = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Filter) || x.Description.Contains(input.Filter))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.IsPublic.HasValue, x => x.IsPublic == input.IsPublic);

        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input)
            .ToListAsync();

        var dtos = await MapToGetListOutputDtosAsync(items);

        return new PagedResultDto<ChatGroupDto>(totalCount, dtos);
    }

    public async Task<ChatGroupDto> ActivateAsync(Guid id)
    {
        var chatGroup = await Repository.GetAsync(id);
        chatGroup.Activate();
        await Repository.UpdateAsync(chatGroup, autoSave: true);

        return await MapToGetOutputDtoAsync(chatGroup);
    }

    public async Task<ChatGroupDto> DeactivateAsync(Guid id)
    {
        var chatGroup = await Repository.GetAsync(id);
        chatGroup.Deactivate();
        await Repository.UpdateAsync(chatGroup, autoSave: true);

        return await MapToGetOutputDtoAsync(chatGroup);
    }
}
