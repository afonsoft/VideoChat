using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Application.Tests.Services.Chat;

/// <summary>
/// Tests for video chat related DTOs and enums used by the SignalR hubs.
/// Hub integration tests require a running SignalR server, so these tests
/// validate the data contracts and enum values used by VideoHub and ChatHub.
/// </summary>
public class VideoChatDtoTests
{
    [Fact]
    public void VideoCallStatus_Should_Have_Expected_Values()
    {
        // Assert
        Enum.IsDefined(typeof(VideoCallStatus), VideoCallStatus.Pending).ShouldBeTrue();
        Enum.IsDefined(typeof(VideoCallStatus), VideoCallStatus.Active).ShouldBeTrue();
        Enum.IsDefined(typeof(VideoCallStatus), VideoCallStatus.Ended).ShouldBeTrue();
        Enum.IsDefined(typeof(VideoCallStatus), VideoCallStatus.Rejected).ShouldBeTrue();
        Enum.IsDefined(typeof(VideoCallStatus), VideoCallStatus.Missed).ShouldBeTrue();
    }

    [Fact]
    public void WebRTCSignalType_Should_Have_Expected_Values()
    {
        // Assert
        Enum.IsDefined(typeof(WebRTCSignalType), WebRTCSignalType.Offer).ShouldBeTrue();
        Enum.IsDefined(typeof(WebRTCSignalType), WebRTCSignalType.Answer).ShouldBeTrue();
        Enum.IsDefined(typeof(WebRTCSignalType), WebRTCSignalType.IceCandidate).ShouldBeTrue();
    }

    [Fact]
    public void NotificationType_Should_Have_Expected_Values()
    {
        // Assert
        Enum.IsDefined(typeof(NotificationType), NotificationType.Message).ShouldBeTrue();
        Enum.IsDefined(typeof(NotificationType), NotificationType.VideoCall).ShouldBeTrue();
        Enum.IsDefined(typeof(NotificationType), NotificationType.GroupInvite).ShouldBeTrue();
        Enum.IsDefined(typeof(NotificationType), NotificationType.System).ShouldBeTrue();
    }

    [Fact]
    public void GroupEventType_Should_Have_Expected_Values()
    {
        // Assert
        Enum.IsDefined(typeof(GroupEventType), GroupEventType.UserJoined).ShouldBeTrue();
        Enum.IsDefined(typeof(GroupEventType), GroupEventType.UserLeft).ShouldBeTrue();
        Enum.IsDefined(typeof(GroupEventType), GroupEventType.UserMuted).ShouldBeTrue();
        Enum.IsDefined(typeof(GroupEventType), GroupEventType.UserUnmuted).ShouldBeTrue();
        Enum.IsDefined(typeof(GroupEventType), GroupEventType.UserBanned).ShouldBeTrue();
        Enum.IsDefined(typeof(GroupEventType), GroupEventType.UserUnbanned).ShouldBeTrue();
    }

    [Fact]
    public void StartVideoCallDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new StartVideoCallDto
        {
            GroupId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid()
        };

        // Assert
        dto.GroupId.ShouldNotBe(Guid.Empty);
        dto.TargetUserId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void AcceptVideoCallDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new AcceptVideoCallDto
        {
            CallId = Guid.NewGuid()
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RejectVideoCallDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new RejectVideoCallDto
        {
            CallId = Guid.NewGuid(),
            Reason = "User is busy"
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
        dto.Reason.ShouldBe("User is busy");
    }

    [Fact]
    public void EndVideoCallDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new EndVideoCallDto
        {
            CallId = Guid.NewGuid()
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void WebRTCSignalDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new WebRTCSignalDto
        {
            CallId = Guid.NewGuid(),
            Type = WebRTCSignalType.Offer,
            Sdp = "v=0\r\no=- 123456 2 IN IP4 127.0.0.1\r\n"
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
        dto.Type.ShouldBe(WebRTCSignalType.Offer);
        dto.Sdp.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void WebRTCIceCandidateDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new WebRTCIceCandidateDto
        {
            CallId = Guid.NewGuid(),
            Candidate = "candidate:123 1 udp 2130706431 10.0.0.1 12345 typ host",
            SdpMid = "0",
            SdpMLineIndex = 0
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
        dto.Candidate.ShouldNotBeNullOrEmpty();
        dto.SdpMid.ShouldBe("0");
        dto.SdpMLineIndex.ShouldBe(0);
    }

    [Fact]
    public void VideoCallRequestDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new VideoCallRequestDto
        {
            CallId = Guid.NewGuid(),
            CallerId = Guid.NewGuid(),
            CallerName = "Test User",
            GroupId = Guid.NewGuid(),
            Status = VideoCallStatus.Pending
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
        dto.CallerId.ShouldNotBe(Guid.Empty);
        dto.CallerName.ShouldBe("Test User");
        dto.GroupId.ShouldNotBe(Guid.Empty);
        dto.Status.ShouldBe(VideoCallStatus.Pending);
    }

    [Fact]
    public void VideoCallResponseDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new VideoCallResponseDto
        {
            CallId = Guid.NewGuid(),
            IsAccepted = true,
            Reason = null
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
        dto.IsAccepted.ShouldBeTrue();
        dto.Reason.ShouldBeNull();
    }

    [Fact]
    public void ScreenShareDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new ScreenShareDto
        {
            CallId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IsSharing = true
        };

        // Assert
        dto.CallId.ShouldNotBe(Guid.Empty);
        dto.UserId.ShouldNotBe(Guid.Empty);
        dto.IsSharing.ShouldBeTrue();
    }

    [Fact]
    public void ChatMessageSignalRDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new ChatMessageSignalRDto
        {
            Id = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            SenderId = Guid.NewGuid(),
            SenderName = "Test User",
            Content = "Hello World",
            Type = MessageType.Text,
            SentAt = DateTime.UtcNow
        };

        // Assert
        dto.Id.ShouldNotBe(Guid.Empty);
        dto.GroupId.ShouldNotBe(Guid.Empty);
        dto.SenderId.ShouldNotBe(Guid.Empty);
        dto.SenderName.ShouldBe("Test User");
        dto.Content.ShouldBe("Hello World");
        dto.Type.ShouldBe(MessageType.Text);
        dto.SentAt.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Fact]
    public void UserTypingDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new UserTypingDto
        {
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            GroupId = Guid.NewGuid(),
            IsTyping = true
        };

        // Assert
        dto.UserId.ShouldNotBe(Guid.Empty);
        dto.UserName.ShouldBe("Test User");
        dto.GroupId.ShouldNotBe(Guid.Empty);
        dto.IsTyping.ShouldBeTrue();
    }

    [Fact]
    public void OnlineStatusDto_Should_Have_Properties()
    {
        // Arrange & Act
        var dto = new OnlineStatusDto
        {
            UserId = Guid.NewGuid(),
            IsOnline = true,
            LastSeen = DateTime.UtcNow
        };

        // Assert
        dto.UserId.ShouldNotBe(Guid.Empty);
        dto.IsOnline.ShouldBeTrue();
        dto.LastSeen.ShouldNotBeNull();
    }
}
