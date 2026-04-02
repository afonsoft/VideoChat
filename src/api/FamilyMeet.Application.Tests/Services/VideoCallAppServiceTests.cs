using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using FamilyMeet.Application.Services;
using FamilyMeet.Application.Contracts.Services;
using FamilyMeet.Application.Contracts.DTOs;
using FamilyMeet.Application.Tests;

namespace FamilyMeet.Application.Tests.Services;

public class VideoCallAppServiceTests : FamilyMeetApplicationTestBase
{
    private readonly Mock<ILogger<VideoCallAppService>> _loggerMock;
    private readonly VideoCallAppService _videoCallAppService;

    public VideoCallAppServiceTests()
    {
        _loggerMock = CreateMock<ILogger<VideoCallAppService>>();
        _videoCallAppService = new VideoCallAppService(_loggerMock.Object);
    }

    [Fact]
    public async Task StartCallAsync_ShouldReturnCallId()
    {
        // Arrange
        var input = new StartVideoCallDto
        {
            GroupId = Guid.NewGuid(),
            InitiatorId = Guid.NewGuid(),
            CallType = "video"
        };

        // Act
        var result = await _videoCallAppService.StartCallAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.CallId.ShouldNotBe(Guid.Empty);
        result.GroupId.ShouldBe(input.GroupId);
        result.InitiatorId.ShouldBe(input.InitiatorId);
        result.CallType.ShouldBe(input.CallType);
        result.Status.ShouldBe("initiated");
        result.StartedAt.ShouldBeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task JoinCallAsync_ShouldReturnValidResponse()
    {
        // Arrange
        var callId = Guid.NewGuid();
        var input = new JoinVideoCallDto
        {
            CallId = callId,
            UserId = Guid.NewGuid(),
            UserName = "Test User"
        };

        // Act
        var result = await _videoCallAppService.JoinCallAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.CallId.ShouldBe(callId);
        result.UserId.ShouldBe(input.UserId);
        result.UserName.ShouldBe(input.UserName);
        result.JoinedAt.ShouldBeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Status.ShouldBe("joined");
    }

    [Fact]
    public async Task EndCallAsync_ShouldUpdateCallStatus()
    {
        // Arrange
        var callId = Guid.NewGuid();
        var input = new EndVideoCallDto
        {
            CallId = callId,
            EndedBy = Guid.NewGuid()
        };

        // Act
        var result = await _videoCallAppService.EndCallAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.CallId.ShouldBe(callId);
        result.EndedBy.ShouldBe(input.EndedBy);
        result.EndedAt.ShouldBeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Status.ShouldBe("ended");
    }
}
