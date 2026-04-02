using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Controllers.Chat;

[Route("api/app/chat-groups")]
public class ChatGroupController : AbpControllerBase, IChatGroupAppService
{
    protected IChatGroupAppService _chatGroupAppService;

    public ChatGroupController(IChatGroupAppService chatGroupAppService)
    {
        _chatGroupAppService = chatGroupAppService;
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<ChatGroupDto>> GetListAsync(ChatGroupListDto input)
    {
        return await _chatGroupAppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual async Task<ChatGroupDto> GetAsync(Guid id)
    {
        return await _chatGroupAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual async Task<ChatGroupDto> CreateAsync(CreateChatGroupDto input)
    {
        return await _chatGroupAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual async Task<ChatGroupDto> UpdateAsync(Guid id, UpdateChatGroupDto input)
    {
        return await _chatGroupAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _chatGroupAppService.DeleteAsync(id);
    }

    [HttpPost("{id}/activate")]
    public async Task<ChatGroupDto> ActivateAsync(Guid id)
    {
        return await _chatGroupAppService.ActivateAsync(id);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<ChatGroupDto> DeactivateAsync(Guid id)
    {
        return await _chatGroupAppService.DeactivateAsync(id);
    }
}
