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

public class ChatMessageAppService : CrudAppService<
    ChatMessage,
    ChatMessageDto,
    Guid,
    ChatMessageListDto,
    CreateChatMessageDto,
    UpdateChatMessageDto>, IChatMessageAppService
{
    public ChatMessageAppService(IRepository<ChatMessage, Guid> repository) : base(repository)
    {
    }

    public override async Task<ChatMessageDto> CreateAsync(CreateChatMessageDto input)
    {
        var chatMessage = new ChatMessage(
            GuidGenerator.Create(),
            input.ChatGroupId,
            CurrentUser.Id ?? Guid.Empty,
            CurrentUser.Name ?? "Anonymous",
            input.Content,
            input.Type,
            input.ReplyToMessageId
        );

        await Repository.InsertAsync(chatMessage, autoSave: true);

        return await MapToGetOutputDtoAsync(chatMessage);
    }

    public async Task<ChatMessageDto> SendAsync(CreateChatMessageDto input)
    {
        return await CreateAsync(input);
    }

    public async Task<ChatMessageDto> EditAsync(Guid id, UpdateChatMessageDto input)
    {
        var chatMessage = await Repository.GetAsync(id);

        // Check if user is the sender
        if (chatMessage.SenderId != (CurrentUser.Id ?? Guid.Empty))
        {
            throw new UnauthorizedAccessException("You can only edit your own messages");
        }

        chatMessage.EditContent(input.Content);
        await Repository.UpdateAsync(chatMessage, autoSave: true);

        return await MapToGetOutputDtoAsync(chatMessage);
    }

    public async Task DeleteSoftAsync(Guid id)
    {
        var chatMessage = await Repository.GetAsync(id);

        // Check if user is the sender
        if (chatMessage.SenderId != (CurrentUser.Id ?? Guid.Empty))
        {
            throw new UnauthorizedAccessException("You can only delete your own messages");
        }

        chatMessage.Delete();
        await Repository.UpdateAsync(chatMessage, autoSave: true);
    }

    public override async Task<PagedResultDto<ChatMessageDto>> GetListAsync(ChatMessageListDto input)
    {
        var queryable = await Repository.GetQueryableAsync();

        queryable = queryable
            .Where(x => x.ChatGroupId == input.ChatGroupId)
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            .WhereIf(input.SenderId.HasValue, x => x.SenderId == input.SenderId)
            .WhereIf(!input.IncludeDeleted.HasValue || !input.IncludeDeleted.Value, x => !x.IsDeleted);

        var totalCount = await queryable.CountAsync();
        var items = await queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input)
            .ToListAsync();

        var dtos = await MapToGetListOutputDtosAsync(items);

        return new PagedResultDto<ChatMessageDto>(totalCount, dtos);
    }

    protected override async Task<ChatMessageDto> MapToGetOutputDtoAsync(ChatMessage entity)
    {
        var dto = await base.MapToGetOutputDtoAsync(entity);

        // Load reply to message if exists
        if (entity.ReplyToMessageId.HasValue)
        {
            var replyMessage = await Repository.FirstOrDefaultAsync(x => x.Id == entity.ReplyToMessageId.Value);
            if (replyMessage != null)
            {
                dto.ReplyToMessage = await MapToGetOutputDtoAsync(replyMessage);
            }
        }

        return dto;
    }
}
