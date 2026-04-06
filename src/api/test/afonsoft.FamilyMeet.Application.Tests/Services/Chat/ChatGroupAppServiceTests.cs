using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Application.Tests.Services.Chat;

public class ChatGroupAppServiceTests : FamilyMeetApplicationTestBase
{
    private readonly IChatGroupAppService _chatGroupAppService;

    public ChatGroupAppServiceTests()
    {
        _chatGroupAppService = GetRequiredService<IChatGroupAppService>();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Groups()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto();

        // Act
        var result = await _chatGroupAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAsync_Should_Return_Group_By_Id()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 50
        };
        
        var createdGroup = await _chatGroupAppService.CreateAsync(input);

        // Act
        var result = await _chatGroupAppService.GetAsync(createdGroup.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdGroup.Id);
        result.Name.ShouldBe("Test Group");
        result.Description.ShouldBe("Test Description");
        result.IsPublic.ShouldBeTrue();
        result.MaxParticipants.ShouldBe(50);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Group()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "New Test Group",
            Description = "New Test Description",
            IsPublic = true,
            MaxParticipants = 25
        };

        // Act
        var result = await _chatGroupAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("New Test Group");
        result.Description.ShouldBe("New Test Description");
        result.IsPublic.ShouldBeTrue();
        result.MaxParticipants.ShouldBe(25);
        result.IsActive.ShouldBeTrue();
        result.CreationTime.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Group()
    {
        // Arrange
        var createInput = new CreateChatGroupDto
        {
            Name = "Original Group",
            Description = "Original Description",
            IsPublic = true,
            MaxParticipants = 50
        };
        
        var createdGroup = await _chatGroupAppService.CreateAsync(createInput);

        var updateInput = new UpdateChatGroupDto
        {
            Name = "Updated Group",
            Description = "Updated Description",
            IsPublic = false,
            MaxParticipants = 30
        };

        // Act
        var result = await _chatGroupAppService.UpdateAsync(createdGroup.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdGroup.Id);
        result.Name.ShouldBe("Updated Group");
        result.Description.ShouldBe("Updated Description");
        result.IsPublic.ShouldBeFalse();
        result.MaxParticipants.ShouldBe(30);
        result.LastModificationTime.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Group()
    {
        // Arrange
        var createInput = new CreateChatGroupDto
        {
            Name = "Group to Delete",
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        };
        
        var createdGroup = await _chatGroupAppService.CreateAsync(createInput);

        // Act
        await _chatGroupAppService.DeleteAsync(createdGroup.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatGroupAppService.GetAsync(createdGroup.Id));
    }

    [Fact]
    public async Task ActivateAsync_Should_Activate_Group()
    {
        // Arrange
        var createInput = new CreateChatGroupDto
        {
            Name = "Inactive Group",
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        };
        
        var createdGroup = await _chatGroupAppService.CreateAsync(createInput);
        
        // First deactivate
        await _chatGroupAppService.DeactivateAsync(createdGroup.Id);

        // Act
        await _chatGroupAppService.ActivateAsync(createdGroup.Id);

        // Assert
        var activatedGroup = await _chatGroupAppService.GetAsync(createdGroup.Id);
        activatedGroup.ShouldNotBeNull();
        activatedGroup.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task DeactivateAsync_Should_Deactivate_Group()
    {
        // Arrange
        var createInput = new CreateChatGroupDto
        {
            Name = "Active Group",
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        };
        
        var createdGroup = await _chatGroupAppService.CreateAsync(createInput);

        // Act
        await _chatGroupAppService.DeactivateAsync(createdGroup.Id);

        // Assert
        var deactivatedGroup = await _chatGroupAppService.GetAsync(createdGroup.Id);
        deactivatedGroup.ShouldNotBeNull();
        deactivatedGroup.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task GetMyGroupsAsync_Should_Return_User_Groups()
    {
        // Arrange
        var group1 = await _chatGroupAppService.CreateAsync(new CreateChatGroupDto
        {
            Name = "My Group 1",
            Description = "Description 1",
            IsPublic = true,
            MaxParticipants = 20
        });
        
        var group2 = await _chatGroupAppService.CreateAsync(new CreateChatGroupDto
        {
            Name = "My Group 2",
            Description = "Description 2",
            IsPublic = false,
            MaxParticipants = 15
        });

        // Act
        var result = await _chatGroupAppService.GetMyGroupsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.ShouldContain(g => g.Name == "My Group 1");
        result.ShouldContain(g => g.Name == "My Group 2");
    }

    [Fact]
    public async Task GetGroupParticipantsAsync_Should_Return_Participants()
    {
        // Arrange
        var group = await _chatGroupAppService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Group with Participants",
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        // Act
        var result = await _chatGroupAppService.GetGroupParticipantsAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task JoinGroupAsync_Should_Add_User_To_Group()
    {
        // Arrange
        var group = await _chatGroupAppService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Public Group",
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        });

        // Act
        await _chatGroupAppService.JoinGroupAsync(group.Id);

        // Assert
        var participants = await _chatGroupAppService.GetGroupParticipantsAsync(group.Id);
        participants.ShouldContain(p => p.UserName == "Admin"); // Current user
    }

    [Fact]
    public async Task LeaveGroupAsync_Should_Remove_User_From_Group()
    {
        // Arrange
        var group = await _chatGroupAppService.CreateAsync(new CreateChatGroupDto
        {
            Name = "Public Group",
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        });
        
        // First join the group
        await _chatGroupAppService.JoinGroupAsync(group.Id);

        // Act
        await _chatGroupAppService.LeaveGroupAsync(group.Id);

        // Assert
        var participants = await _chatGroupAppService.GetGroupParticipantsAsync(group.Id);
        participants.ShouldNotContain(p => p.UserName == "Admin"); // Current user
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Name_Length()
    {
        // Arrange
        var longName = new string('A', 129); // Exceeds max length of 128
        var input = new CreateChatGroupDto
        {
            Name = longName,
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatGroupAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Description_Length()
    {
        // Arrange
        var longDescription = new string('A', 501); // Exceeds max length of 500
        var input = new CreateChatGroupDto
        {
            Name = "Valid Name",
            Description = longDescription,
            IsPublic = true,
            MaxParticipants = 10
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatGroupAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Max_Participants()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "Valid Name",
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 0 // Invalid (should be > 0)
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatGroupAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Name_Required()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "", // Empty name
            Description = "Description",
            IsPublic = true,
            MaxParticipants = 10
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatGroupAppService.CreateAsync(input));
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_Exception_When_Group_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateInput = new UpdateChatGroupDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsPublic = true,
            MaxParticipants = 20
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatGroupAppService.UpdateAsync(nonExistentId, updateInput));
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_Exception_When_Group_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatGroupAppService.DeleteAsync(nonExistentId));
    }

    [Fact]
    public async Task GetAsync_Should_Throw_Exception_When_Group_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatGroupAppService.GetAsync(nonExistentId));
    }

    [Fact]
    public async Task Create_Private_Group_Should_Set_IsPublic_False()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "Private Group",
            Description = "Private Description",
            IsPublic = false,
            MaxParticipants = 5
        };

        // Act
        var result = await _chatGroupAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.IsPublic.ShouldBeFalse();
        result.Name.ShouldBe("Private Group");
    }

    [Fact]
    public async Task Create_Public_Group_Should_Set_IsPublic_True()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "Public Group",
            Description = "Public Description",
            IsPublic = true,
            MaxParticipants = 100
        };

        // Act
        var result = await _chatGroupAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.IsPublic.ShouldBeTrue();
        result.Name.ShouldBe("Public Group");
    }

    [Fact]
    public async Task UpdateAsync_Should_Preserve_Creation_Time()
    {
        // Arrange
        var createInput = new CreateChatGroupDto
        {
            Name = "Original Group",
            Description = "Original Description",
            IsPublic = true,
            MaxParticipants = 50
        };
        
        var createdGroup = await _chatGroupAppService.CreateAsync(createInput);
        var originalCreationTime = createdGroup.CreationTime;

        var updateInput = new UpdateChatGroupDto
        {
            Name = "Updated Group",
            Description = "Updated Description",
            IsPublic = false,
            MaxParticipants = 30
        };

        // Act
        var result = await _chatGroupAppService.UpdateAsync(createdGroup.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.CreationTime.ShouldBe(originalCreationTime);
        result.LastModificationTime.ShouldNotBeNull();
    }
}
