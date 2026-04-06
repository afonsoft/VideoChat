using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Xunit;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Chat.Controllers;

public class ChatGroupControllerTests : FamilyMeetApplicationTestBase
{
    private readonly IChatGroupAppService _chatGroupAppService;
    private readonly ChatGroupController _controller;

    public ChatGroupControllerTests()
    {
        _chatGroupAppService = GetRequiredService<IChatGroupAppService>();
        _controller = new ChatGroupController(_chatGroupAppService);
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Result()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto();
        var expectedGroups = new PagedResultDto<ChatGroupDto>(
            new[] { new ChatGroupDto { Id = Guid.NewGuid(), Name = "Test Group" } },
            1
        );

        _chatGroupAppService.GetListAsync(Arg.Any<PagedAndSortedResultRequestDto>())
            .Returns(expectedGroups);

        // Act
        var result = await _controller.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Test Group");

        await _chatGroupAppService.Received(1).GetListAsync(Arg.Any<PagedAndSortedResultRequestDto>());
    }

    [Fact]
    public async Task GetAsync_Should_Return_Group_By_Id()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var expectedGroup = new ChatGroupDto
        {
            Id = groupId,
            Name = "Test Group",
            Description = "Test Description"
        };

        _chatGroupAppService.GetAsync(groupId)
            .Returns(expectedGroup);

        // Act
        var result = await _controller.GetAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(groupId);
        result.Name.ShouldBe("Test Group");
        result.Description.ShouldBe("Test Description");

        await _chatGroupAppService.Received(1).GetAsync(groupId);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Group()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "New Group",
            Description = "New Description",
            IsPublic = true,
            MaxParticipants = 50
        };

        var expectedGroup = new ChatGroupDto
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Description = input.Description,
            IsPublic = input.IsPublic,
            MaxParticipants = input.MaxParticipants,
            IsActive = true
        };

        _chatGroupAppService.CreateAsync(input)
            .Returns(expectedGroup);

        // Act
        var result = await _controller.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);
        result.Description.ShouldBe(input.Description);
        result.IsPublic.ShouldBe(input.IsPublic);
        result.MaxParticipants.ShouldBe(input.MaxParticipants);
        result.IsActive.ShouldBeTrue();

        await _chatGroupAppService.Received(1).CreateAsync(input);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Group()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var input = new UpdateChatGroupDto
        {
            Name = "Updated Group",
            Description = "Updated Description",
            IsPublic = false,
            MaxParticipants = 25
        };

        var expectedGroup = new ChatGroupDto
        {
            Id = groupId,
            Name = input.Name,
            Description = input.Description,
            IsPublic = input.IsPublic,
            MaxParticipants = input.MaxParticipants
        };

        _chatGroupAppService.UpdateAsync(groupId, input)
            .Returns(expectedGroup);

        // Act
        var result = await _controller.UpdateAsync(groupId, input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(groupId);
        result.Name.ShouldBe(input.Name);
        result.Description.ShouldBe(input.Description);
        result.IsPublic.ShouldBe(input.IsPublic);
        result.MaxParticipants.ShouldBe(input.MaxParticipants);

        await _chatGroupAppService.Received(1).UpdateAsync(groupId, input);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Group()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        await _controller.DeleteAsync(groupId);

        // Assert
        await _chatGroupAppService.Received(1).DeleteAsync(groupId);
    }

    [Fact]
    public async Task ActivateAsync_Should_Activate_Group()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        await _controller.ActivateAsync(groupId);

        // Assert
        await _chatGroupAppService.Received(1).ActivateAsync(groupId);
    }

    [Fact]
    public async Task DeactivateAsync_Should_Deactivate_Group()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        await _controller.DeactivateAsync(groupId);

        // Assert
        await _chatGroupAppService.Received(1).DeactivateAsync(groupId);
    }

    [Fact]
    public async Task GetMyGroupsAsync_Should_Return_User_Groups()
    {
        // Arrange
        var expectedGroups = new ChatGroupDto[]
        {
            new() { Id = Guid.NewGuid(), Name = "My Group 1" },
            new() { Id = Guid.NewGuid(), Name = "My Group 2" }
        };

        _chatGroupAppService.GetMyGroupsAsync()
            .Returns(expectedGroups);

        // Act
        var result = await _controller.GetMyGroupsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("My Group 1");
        result[1].Name.ShouldBe("My Group 2");

        await _chatGroupAppService.Received(1).GetMyGroupsAsync();
    }

    [Fact]
    public async Task GetGroupParticipantsAsync_Should_Return_Participants()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var expectedParticipants = new ChatParticipantDto[]
        {
            new() { Id = Guid.NewGuid(), UserName = "User 1" },
            new() { Id = Guid.NewGuid(), UserName = "User 2" }
        };

        _chatGroupAppService.GetGroupParticipantsAsync(groupId)
            .Returns(expectedParticipants);

        // Act
        var result = await _controller.GetGroupParticipantsAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].UserName.ShouldBe("User 1");
        result[1].UserName.ShouldBe("User 2");

        await _chatGroupAppService.Received(1).GetGroupParticipantsAsync(groupId);
    }

    [Fact]
    public async Task JoinGroupAsync_Should_Add_User_To_Group()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        await _controller.JoinGroupAsync(groupId);

        // Assert
        await _chatGroupAppService.Received(1).JoinGroupAsync(groupId);
    }

    [Fact]
    public async Task LeaveGroupAsync_Should_Remove_User_From_Group()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Act
        await _controller.LeaveGroupAsync(groupId);

        // Assert
        await _chatGroupAppService.Received(1).LeaveGroupAsync(groupId);
    }
}
