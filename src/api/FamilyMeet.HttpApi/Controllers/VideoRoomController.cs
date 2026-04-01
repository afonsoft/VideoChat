using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FamilyMeet.Application.Contracts.DTOs;
using FamilyMeet.Application.Contracts.Services;
using FamilyMeet.HttpApi.Services;

namespace FamilyMeet.HttpApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoRoomController : ControllerBase
{
    private readonly ILogger<VideoRoomController> _logger;
    private readonly IVideoCallAppService _videoCallAppService;
    private readonly IConnectionManager _connectionManager;

    public VideoRoomController(
        ILogger<VideoRoomController> logger,
        IVideoCallAppService videoCallAppService,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _videoCallAppService = videoCallAppService;
        _connectionManager = connectionManager;
    }

    [HttpPost("create")]
    public async Task<ActionResult<VideoRoomDto>> CreateRoom([FromBody] CreateVideoRoomDto input)
    {
        try
        {
            var room = new VideoRoomDto
            {
                Id = Guid.NewGuid(),
                Name = input.Name,
                Description = input.Description,
                MaxParticipants = input.MaxParticipants,
                IsPrivate = input.IsPrivate,
                Password = input.IsPrivate ? input.Password : null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = input.CreatedBy,
                IsActive = true,
                Settings = new VideoRoomSettingsDto
                {
                    AllowScreenShare = input.AllowScreenShare,
                    RequirePassword = input.IsPrivate,
                    AutoRecord = input.AutoRecord,
                    MaxDuration = input.MaxDuration,
                    Quality = input.Quality
                }
            };

            _logger.LogInformation("Video room {RoomId} created by {UserId}", room.Id, input.CreatedBy);
            return Ok(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video room");
            return BadRequest(new { error = "Failed to create room" });
        }
    }

    [HttpGet("{roomId}/info")]
    public async Task<ActionResult<VideoRoomDto>> GetRoomInfo(Guid roomId)
    {
        try
        {
            // In a real implementation, get from database
            var room = new VideoRoomDto
            {
                Id = roomId,
                Name = "Sample Room",
                Description = "Sample Description",
                MaxParticipants = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            };

            return Ok(room);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting room info for {RoomId}", roomId);
            return BadRequest(new { error = "Failed to get room info" });
        }
    }

    [HttpPost("{roomId}/join")]
    public async Task<ActionResult<JoinVideoRoomResponseDto>> JoinRoom(Guid roomId, [FromBody] JoinVideoRoomDto input)
    {
        try
        {
            // Validate room exists and user can join
            var participants = await _videoCallAppService.GetCallParticipantsAsync(roomId);
            
            if (participants.Count >= 50) // Max participants check
            {
                return BadRequest(new { error = "Room is full" });
            }

            // Add participant to call
            var joinCallDto = new JoinCallDto
            {
                GroupId = roomId,
                UserId = input.UserId,
                UserName = input.UserName,
                HasAudio = input.HasAudio,
                HasVideo = input.HasVideo
            };

            var callInfo = await _videoCallAppService.JoinCallAsync(joinCallDto);

            var response = new JoinVideoRoomResponseDto
            {
                RoomId = roomId,
                UserId = input.UserId,
                Token = GenerateRoomToken(roomId, input.UserId),
                Participants = callInfo.Participants,
                Settings = new VideoRoomSettingsDto
                {
                    AllowScreenShare = true,
                    RequirePassword = false,
                    AutoRecord = false,
                    MaxDuration = 120,
                    Quality = "HD"
                }
            };

            _logger.LogInformation("User {UserId} joined video room {RoomId}", input.UserId, roomId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining video room {RoomId}", roomId);
            return BadRequest(new { error = "Failed to join room" });
        }
    }

    [HttpPost("{roomId}/leave")]
    public async Task<ActionResult> LeaveRoom(Guid roomId, [FromBody] LeaveVideoRoomDto input)
    {
        try
        {
            var leaveCallDto = new LeaveCallDto
            {
                GroupId = roomId,
                UserId = input.UserId
            };

            await _videoCallAppService.LeaveCallAsync(leaveCallDto);

            _logger.LogInformation("User {UserId} left video room {RoomId}", input.UserId, roomId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving video room {RoomId}", roomId);
            return BadRequest(new { error = "Failed to leave room" });
        }
    }

    [HttpPost("{roomId}/record/start")]
    public async Task<ActionResult> StartRecording(Guid roomId)
    {
        try
        {
            // Implement recording logic
            _logger.LogInformation("Recording started for room {RoomId}", roomId);
            return Ok(new { message = "Recording started", recordingId = Guid.NewGuid() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording for room {RoomId}", roomId);
            return BadRequest(new { error = "Failed to start recording" });
        }
    }

    [HttpPost("{roomId}/record/stop")]
    public async Task<ActionResult> StopRecording(Guid roomId, [FromBody] StopRecordingDto input)
    {
        try
        {
            // Implement stop recording logic
            _logger.LogInformation("Recording stopped for room {RoomId}, recording {RecordingId}", roomId, input.RecordingId);
            return Ok(new { message = "Recording stopped", fileUrl = $"/recordings/{input.RecordingId}.mp4" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording for room {RoomId}", roomId);
            return BadRequest(new { error = "Failed to stop recording" });
        }
    }

    [HttpGet("{roomId}/recordings")]
    public async Task<ActionResult<List<RecordingDto>>> GetRecordings(Guid roomId)
    {
        try
        {
            var recordings = new List<RecordingDto>
            {
                new RecordingDto
                {
                    Id = Guid.NewGuid(),
                    RoomId = roomId,
                    FileName = "recording_001.mp4",
                    Duration = TimeSpan.FromMinutes(45),
                    Size = 1024 * 1024 * 50, // 50MB
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    Url = $"/recordings/{roomId}/recording_001.mp4"
                }
            };

            return Ok(recordings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recordings for room {RoomId}", roomId);
            return BadRequest(new { error = "Failed to get recordings" });
        }
    }

    private string GenerateRoomToken(Guid roomId, Guid userId)
    {
        // Generate a token for the room (simplified)
        return $"{roomId}_{userId}_{DateTime.UtcNow.Ticks}";
    }
}

// Additional DTOs for Video Room Management
public class VideoRoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public bool IsPrivate { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public VideoRoomSettingsDto Settings { get; set; } = new();
}

public class VideoRoomSettingsDto
{
    public bool AllowScreenShare { get; set; }
    public bool RequirePassword { get; set; }
    public bool AutoRecord { get; set; }
    public int MaxDuration { get; set; } // minutes
    public string Quality { get; set; } = "HD"; // SD, HD, FullHD, 4K
}

public class CreateVideoRoomDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxParticipants { get; set; } = 50;
    public bool IsPrivate { get; set; }
    public string? Password { get; set; }
    public Guid CreatedBy { get; set; }
    public bool AllowScreenShare { get; set; } = true;
    public bool AutoRecord { get; set; } = false;
    public int MaxDuration { get; set; } = 120; // minutes
    public string Quality { get; set; } = "HD";
}

public class JoinVideoRoomDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool HasAudio { get; set; } = true;
    public bool HasVideo { get; set; } = true;
    public string? Password { get; set; }
}

public class JoinVideoRoomResponseDto
{
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public List<CallParticipantDto> Participants { get; set; } = new();
    public VideoRoomSettingsDto Settings { get; set; } = new();
}

public class LeaveVideoRoomDto
{
    public Guid UserId { get; set; }
}

public class StopRecordingDto
{
    public Guid RecordingId { get; set; }
}

public class RecordingDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Url { get; set; } = string.Empty;
}
