using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Application.Tests.Services.Chat;

public class ChatParticipantAppServiceTests : FamilyMeetApplicationTestBase
{
    private readonly IChatParticipantAppService _chatParticipantAppService;

    public ChatParticipantAppServiceTests()
    {
        _chatParticipantAppService = GetRequiredService<IChatParticipantAppService>();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Participants()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        
        // Create a group first
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        // Act
        var result = await _chatParticipantAppService.GetListAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Participant_By_Id()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        // Add current user as participant
        await groupService.JoinGroupAsync(group.Id);

        // Get participants
        var participants = await _chatParticipantAppService.GetListAsync(group.Id);
        if (participants.Count > 0)
        {
            var participantId = participants[0].Id;

            // Act
            var result = await _chatParticipantAppService.GetAsync(participantId);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(participantId);
            result.ChatGroupId.ShouldBe(group.Id);
        }
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Participant_To_Group()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var input = new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            IsCreator = false
        };

        // Act
        var result = await _chatParticipantAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.ChatGroupId.ShouldBe(group.Id);
        result.UserId.ShouldBe(input.UserId);
        result.UserName.ShouldBe("Test User");
        result.IsCreator.ShouldBeFalse();
        result.IsOnline.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Participant_Status()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var createInput = new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            IsCreator = false
        };

        var createdParticipant = await _chatParticipantAppService.CreateAsync(createInput);

        var updateInput = new UpdateChatParticipantDto
        {
            IsOnline = true,
            IsMuted = false,
            IsBanned = false
        };

        // Act
        var result = await _chatParticipantAppService.UpdateAsync(createdParticipant.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdParticipant.Id);
        result.IsOnline.ShouldBeTrue();
        result.IsMuted.ShouldBeFalse();
        result.IsBanned.ShouldBeFalse();
        result.LastSeenAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Participant_From_Group()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var createInput = new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            IsCreator = false
        };

        var createdParticipant = await _chatParticipantAppService.CreateAsync(createInput);

        // Act
        await _chatParticipantAppService.DeleteAsync(createdParticipant.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatParticipantAppService.GetAsync(createdParticipant.Id));
    }

    [Fact]
    public async Task GetGroupParticipantsAsync_Should_Return_All_Group_Participants()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        // Add multiple participants
        var participant1 = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "User 1",
            IsCreator = true
        });

        var participant2 = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "User 2",
            IsCreator = false
        });

        // Act
        var result = await _chatParticipantAppService.GetGroupParticipantsAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.ShouldContain(p => p.UserName == "User 1");
        result.ShouldContain(p => p.UserName == "User 2");
    }

    [Fact]
    public async Task GetOnlineParticipantsAsync_Should_Return_Only_Online_Participants()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        // Create participants
        var participant1 = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "Online User",
            IsCreator = false
        });

        var participant2 = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "Offline User",
            IsCreator = false
        });

        // Set first participant as online
        await _chatParticipantAppService.UpdateAsync(participant1.Id, new UpdateChatParticipantDto
        {
            IsOnline = true
        });

        // Set second participant as offline
        await _chatParticipantAppService.UpdateAsync(participant2.Id, new UpdateChatParticipantDto
        {
            IsOnline = false
        });

        // Act
        var result = await _chatParticipantAppService.GetOnlineParticipantsAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(1);
        result.ShouldContain(p => p.UserName == "Online User");
        result.ShouldNotContain(p => p.UserName == "Offline User");
    }

    [Fact]
    public async Task MuteParticipantAsync_Should_Mute_Participant()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var participant = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "User to Mute",
            IsCreator = false
        });

        // Act
        await _chatParticipantAppService.MuteParticipantAsync(participant.Id);

        // Assert
        var mutedParticipant = await _chatParticipantAppService.GetAsync(participant.Id);
        mutedParticipant.ShouldNotBeNull();
        mutedParticipant.IsMuted.ShouldBeTrue();
    }

    [Fact]
    public async Task UnmuteParticipantAsync_Should_Unmute_Participant()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var participant = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "User to Unmute",
            IsCreator = false
        });

        // First mute the participant
        await _chatParticipantAppService.MuteParticipantAsync(participant.Id);

        // Act
        await _chatParticipantAppService.UnmuteParticipantAsync(participant.Id);

        // Assert
        var unmutedParticipant = await _chatParticipantAppService.GetAsync(participant.Id);
        unmutedParticipant.ShouldNotBeNull();
        unmutedParticipant.IsMuted.ShouldBeFalse();
    }

    [Fact]
    public async Task BanParticipantAsync_Should_Ban_Participant()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var participant = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "User to Ban",
            IsCreator = false
        });

        var banDuration = TimeSpan.FromHours(1);

        // Act
        await _chatParticipantAppService.BanParticipantAsync(participant.Id, banDuration);

        // Assert
        var bannedParticipant = await _chatParticipantAppService.GetAsync(participant.Id);
        bannedParticipant.ShouldNotBeNull();
        bannedParticipant.IsBanned.ShouldBeTrue();
        bannedParticipant.BannedUntil.ShouldNotBeNull();
        bannedParticipant.BannedUntil.Value.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public async Task UnbanParticipantAsync_Should_Unban_Participant()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var participant = await _chatParticipantAppService.CreateAsync(new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "User to Unban",
            IsCreator = false
        });

        // First ban the participant
        await _chatParticipantAppService.BanParticipantAsync(participant.Id, TimeSpan.FromHours(1));

        // Act
        await _chatParticipantAppService.UnbanParticipantAsync(participant.Id);

        // Assert
        var unbannedParticipant = await _chatParticipantAppService.GetAsync(participant.Id);
        unbannedParticipant.ShouldNotBeNull();
        unbannedParticipant.IsBanned.ShouldBeFalse();
        unbannedParticipant.BannedUntil.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_UserName_Length()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var longUserName = new string('A', 129); // Exceeds max length of 128
        var input = new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = longUserName,
            IsCreator = false
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatParticipantAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_UserName_Required()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var input = new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.NewGuid(),
            UserName = "", // Empty user name
            IsCreator = false
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatParticipantAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_ChatGroupId()
    {
        // Arrange
        var invalidGroupId = Guid.Empty;
        var input = new CreateChatParticipantDto
        {
            ChatGroupId = invalidGroupId,
            UserId = Guid.NewGuid(),
            UserName = "Test User",
            IsCreator = false
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatParticipantAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_UserId()
    {
        // Arrange
        var groupService = GetRequiredService<IChatGroupAppService>();
        var group = await groupService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        var input = new CreateChatParticipantDto
        {
            ChatGroupId = group.Id,
            UserId = Guid.Empty, // Invalid user ID
            UserName = "Test User",
            IsCreator = false
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatParticipantAppService.CreateAsync(input));
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_Exception_When_Participant_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateInput = new UpdateChatParticipantDto
        {
            IsOnline = true,
            IsMuted = false,
            IsBanned = false
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatParticipantAppService.UpdateAsync(nonExistentId, updateInput));
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_Exception_When_Participant_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatParticipantAppService.DeleteAsync(nonExistentId));
    }
}
