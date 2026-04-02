using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Application.Chat;

public abstract class ChatGroupAppServiceTests<TModule> : FamilyMeetApplicationTestBase<TModule>
    where TModule : IAbpModule
{
    private readonly IChatGroupAppService _chatGroupAppService;
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository;

    protected ChatGroupAppServiceTests()
    {
        _chatGroupAppService = GetRequiredService<IChatGroupAppService>();
        _chatGroupRepository = GetRequiredService<IRepository<ChatGroup, Guid>>();
    }

    [Fact]
    public async Task Should_Create_Group()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 50
        };

        // Act
        var result = await _chatGroupAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);
        result.Description.ShouldBe(input.Description);
        result.IsPublic.ShouldBe(input.IsPublic);
        result.MaxParticipants.ShouldBe(input.MaxParticipants);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Get_List_Of_Groups()
    {
        // Arrange
        await CreateTestGroupAsync("Group 1", "Description 1");
        await CreateTestGroupAsync("Group 2", "Description 2");

        // Act
        var result = await _chatGroupAppService.GetListAsync(new ChatGroupListDto());

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Should_Filter_Groups_By_Name()
    {
        // Arrange
        await CreateTestGroupAsync("Test Group 1", "Description 1");
        await CreateTestGroupAsync("Other Group", "Description 2");

        // Act
        var result = await _chatGroupAppService.GetListAsync(
            new ChatGroupListDto { Filter = "Test" });

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        result.Items.ShouldAllBe(x => x.Name.Contains("Test"));
    }

    [Fact]
    public async Task Should_Get_Single_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");

        // Act
        var result = await _chatGroupAppService.GetAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Group");
        result.Description.ShouldBe("Test Description");
    }

    [Fact]
    public async Task Should_Update_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Original Name", "Original Description");
        var input = new UpdateChatGroupDto
        {
            Name = "Updated Name",
            Description = "Updated Description",
            IsPublic = false,
            MaxParticipants = 75
        };

        // Act
        var result = await _chatGroupAppService.UpdateAsync(group.Id, input);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);
        result.Description.ShouldBe(input.Description);
        result.IsPublic.ShouldBe(input.IsPublic);
        result.MaxParticipants.ShouldBe(input.MaxParticipants);
    }

    [Fact]
    public async Task Should_Delete_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");

        // Act
        await _chatGroupAppService.DeleteAsync(group.Id);

        // Assert
        var deletedGroup = await _chatGroupRepository.FindAsync(group.Id);
        deletedGroup.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Activate_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        group.Deactivate();
        await _chatGroupRepository.UpdateAsync(group);

        // Act
        var result = await _chatGroupAppService.ActivateAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Deactivate_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");

        // Act
        var result = await _chatGroupAppService.DeactivateAsync(group.Id);

        // Assert
        result.ShouldNotBeNull();
        result.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Handle_Max_Participants_Validation()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            IsPublic = true,
            MaxParticipants = 2000 // Exceeds max limit
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatGroupAppService.CreateAsync(input));
    }

    private async Task<ChatGroup> CreateTestGroupAsync(string name, string description)
    {
        var group = new ChatGroup(
            GuidGenerator.Create(),
            name,
            description,
            true,
            50
        );

        return await _chatGroupRepository.InsertAsync(group);
    }
}
