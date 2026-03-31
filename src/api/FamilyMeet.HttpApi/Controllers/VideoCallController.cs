using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FamilyMeet.Application.Contracts.DTOs;
using FamilyMeet.Application.Contracts.Services;

namespace FamilyMeet.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoCallController : ControllerBase
{
    private readonly ILogger<VideoCallController> _logger;
    private readonly IVideoCallAppService _videoCallAppService;

    public VideoCallController(ILogger<VideoCallController> logger, IVideoCallAppService videoCallAppService)
    {
        _logger = logger;
        _videoCallAppService = videoCallAppService;
    }

    [HttpPost("join")]
    public async Task<ActionResult<CallInfoDto>> JoinCall([FromBody] JoinCallDto input)
    {
        try
        {
            var result = await _videoCallAppService.JoinCallAsync(input);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining call in group {GroupId}", input.GroupId);
            return BadRequest(new { error = "Failed to join call" });
        }
    }

    [HttpPost("leave")]
    public async Task<ActionResult> LeaveCall([FromBody] LeaveCallDto input)
    {
        try
        {
            await _videoCallAppService.LeaveCallAsync(input);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving call in group {GroupId}", input.GroupId);
            return BadRequest(new { error = "Failed to leave call" });
        }
    }

    [HttpGet("{groupId}/info")]
    public async Task<ActionResult<CallInfoDto>> GetCallInfo(Guid groupId)
    {
        try
        {
            var result = await _videoCallAppService.GetCallInfoAsync(groupId);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting call info for group {GroupId}", groupId);
            return BadRequest(new { error = "Failed to get call info" });
        }
    }

    [HttpPost("participant/status")]
    public async Task<ActionResult> UpdateParticipantStatus([FromBody] UpdateParticipantStatusDto input)
    {
        try
        {
            await _videoCallAppService.UpdateParticipantStatusAsync(input);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant status in group {GroupId}", input.GroupId);
            return BadRequest(new { error = "Failed to update participant status" });
        }
    }

    [HttpGet("{groupId}/participants")]
    public async Task<ActionResult<List<CallParticipantDto>>> GetCallParticipants(Guid groupId)
    {
        try
        {
            var result = await _videoCallAppService.GetCallParticipantsAsync(groupId);
            return Ok(result);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting call participants for group {GroupId}", groupId);
            return BadRequest(new { error = "Failed to get call participants" });
        }
    }
}
