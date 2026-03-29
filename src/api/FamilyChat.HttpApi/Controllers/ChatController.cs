using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FamilyChat.Application.Contracts.DTOs;
using FamilyChat.Application.Contracts.Services;

namespace FamilyChat.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatAppService _chatAppService;

    public ChatController(ILogger<ChatController> logger, IChatAppService chatAppService)
    {
        _logger = logger;
        _chatAppService = chatAppService;
    }

    [HttpPost("groups")]
    public async Task<ActionResult<ChatGroupDto>> CreateGroup([FromBody] CreateChatGroupDto input)
    {
        try
        {
            var result = await _chatAppService.CreateGroupAsync(input);
            return CreatedAtAction(nameof(GetGroup), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return BadRequest(new { error = "Failed to create group" });
        }
    }

    [HttpGet("groups/{id}")]
    public async Task<ActionResult<ChatGroupDto>> GetGroup(Guid id)
    {
        try
        {
            var result = await _chatAppService.GetGroupAsync(id);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", id);
            return BadRequest(new { error = "Failed to get group" });
        }
    }

    [HttpPut("groups/{id}")]
    public async Task<ActionResult<ChatGroupDto>> UpdateGroup(Guid id, [FromBody] UpdateChatGroupDto input)
    {
        try
        {
            var result = await _chatAppService.UpdateGroupAsync(id, input);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", id);
            return BadRequest(new { error = "Failed to update group" });
        }
    }

    [HttpDelete("groups/{id}")]
    public async Task<ActionResult> DeleteGroup(Guid id)
    {
        try
        {
            await _chatAppService.DeleteGroupAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", id);
            return BadRequest(new { error = "Failed to delete group" });
        }
    }

    [HttpGet("groups/my-groups/{userId}")]
    public async Task<ActionResult<List<ChatGroupDto>>> GetUserGroups(Guid userId)
    {
        try
        {
            var result = await _chatAppService.GetUserGroupsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups {UserId}", userId);
            return BadRequest(new { error = "Failed to get user groups" });
        }
    }

    [HttpPost("groups/join")]
    public async Task<ActionResult<ChatGroupDto>> JoinGroup([FromBody] JoinGroupDto input)
    {
        try
        {
            var result = await _chatAppService.JoinGroupAsync(input);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group {GroupId}", input.GroupId);
            return BadRequest(new { error = "Failed to join group" });
        }
    }

    [HttpPost("groups/leave")]
    public async Task<ActionResult> LeaveGroup([FromBody] LeaveGroupDto input)
    {
        try
        {
            await _chatAppService.LeaveGroupAsync(input);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupId}", input.GroupId);
            return BadRequest(new { error = "Failed to leave group" });
        }
    }

    [HttpGet("groups/{id}/members")]
    public async Task<ActionResult<List<ChatGroupMemberDto>>> GetGroupMembers(Guid id)
    {
        try
        {
            var result = await _chatAppService.GetGroupMembersAsync(id);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group members {GroupId}", id);
            return BadRequest(new { error = "Failed to get group members" });
        }
    }
}
