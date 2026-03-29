using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FamilyChat.Application.Contracts.DTOs;
using FamilyChat.Application.Contracts.Services;

namespace FamilyChat.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly IChatMessageAppService _messageAppService;

    public MessagesController(ILogger<MessagesController> logger, IChatMessageAppService messageAppService)
    {
        _logger = logger;
        _messageAppService = messageAppService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageDto input)
    {
        try
        {
            var result = await _messageAppService.SendMessageAsync(input);
            return CreatedAtAction(nameof(GetMessage), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return BadRequest(new { error = "Failed to send message" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChatMessageDto>> GetMessage(Guid id)
    {
        try
        {
            var result = await _messageAppService.GetMessageAsync(id);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting message {MessageId}", id);
            return BadRequest(new { error = "Failed to get message" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChatMessageDto>> EditMessage(Guid id, [FromBody] EditMessageDto input)
    {
        try
        {
            var result = await _messageAppService.EditMessageAsync(id, input);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing message {MessageId}", id);
            return BadRequest(new { error = "Failed to edit message" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(Guid id)
    {
        try
        {
            await _messageAppService.DeleteMessageAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", id);
            return BadRequest(new { error = "Failed to delete message" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<MessagePagedResultDto>> GetMessages([FromQuery] GetMessagesDto input)
    {
        try
        {
            var result = await _messageAppService.GetMessagesAsync(input);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { error = "Invalid group ID" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages for group {GroupId}", input.ChatGroupId);
            return BadRequest(new { error = "Failed to get messages" });
        }
    }
}
