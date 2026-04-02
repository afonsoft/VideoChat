using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Controllers.Chat;

[Route("api/app/chat-messages")]
public class ChatMessageController : AbpControllerBase, IChatMessageAppService
{
    protected IChatMessageAppService _chatMessageAppService;

    public ChatMessageController(IChatMessageAppService chatMessageAppService)
    {
        _chatMessageAppService = chatMessageAppService;
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<ChatMessageDto>> GetListAsync(ChatMessageListDto input)
    {
        return await _chatMessageAppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual async Task<ChatMessageDto> GetAsync(Guid id)
    {
        return await _chatMessageAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual async Task<ChatMessageDto> CreateAsync(CreateChatMessageDto input)
    {
        return await _chatMessageAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual async Task<ChatMessageDto> UpdateAsync(Guid id, UpdateChatMessageDto input)
    {
        return await _chatMessageAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _chatMessageAppService.DeleteAsync(id);
    }

    [HttpPost("send")]
    public async Task<ChatMessageDto> SendAsync(CreateChatMessageDto input)
    {
        return await _chatMessageAppService.SendAsync(input);
    }

    [HttpPut("{id}/edit")]
    public async Task<ChatMessageDto> EditAsync(Guid id, UpdateChatMessageDto input)
    {
        return await _chatMessageAppService.EditAsync(id, input);
    }

    [HttpDelete("{id}/soft")]
    public async Task DeleteSoftAsync(Guid id)
    {
        await _chatMessageAppService.DeleteSoftAsync(id);
    }
}
