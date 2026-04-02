using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Volo.Abp.Domain.Repositories;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Domain.Chat;

public abstract class ChatGroupTests<TModule> : FamilyMeetDomainTestBase<TModule>
    where TModule : IAbpModule
{
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository;

    protected ChatGroupTests()
    {
        _chatGroupRepository = GetRequiredService<IRepository<ChatGroup, Guid>>();
    }

    [Fact]
    public async Task Should_Create_Group_With_Valid_Data()
    {
        // Arrange
        var name = "Test Group";
        var description = "Test Description";

        // Act
        var group = new ChatGroup(
            GuidGenerator.Create(),
            name,
            description,
            true,
            50
        );

        // Assert
        group.ShouldNotBeNull();
        group.Name.ShouldBe(name);
        group.Description.ShouldBe(description);
        group.IsPublic.ShouldBeTrue();
        group.MaxParticipants.ShouldBe(50);
        group.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Update_Group_Name()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Original Name", "Original Description");
        var newName = "Updated Name";

        // Act
        group.UpdateName(newName);

        // Assert
        group.Name.ShouldBe(newName);
    }

    [Fact]
    public async Task Should_Update_Group_Description()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Original Description");
        var newDescription = "Updated Description";

        // Act
        group.UpdateDescription(newDescription);

        // Assert
        group.Description.ShouldBe(newDescription);
    }

    [Fact]
    public async Task Should_Set_Last_Message_Time()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var messageTime = DateTime.UtcNow;

        // Act
        group.SetLastMessageTime(messageTime);

        // Assert
        group.LastMessageAt.ShouldBe(messageTime);
    }

    [Fact]
    public async Task Should_Activate_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        group.Deactivate();

        // Act
        group.Activate();

        // Assert
        group.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Deactivate_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");

        // Act
        group.Deactivate();

        // Assert
        group.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Handle_Max_Participants_Limit()
    {
        // Arrange & Act
        var group = new ChatGroup(
            GuidGenerator.Create(),
            "Test Group",
            "Test Description",
            true,
            ChatConstants.ChatGroup.MaxMaxParticipants
        );

        // Assert
        group.MaxParticipants.ShouldBe(ChatConstants.ChatGroup.MaxMaxParticipants);
    }

    [Fact]
    public async Task Should_Persist_Group_To_Database()
    {
        // Arrange
        var group = new ChatGroup(
            GuidGenerator.Create(),
            "Test Group",
            "Test Description",
            true,
            50
        );

        // Act
        await _chatGroupRepository.InsertAsync(group);

        // Assert
        var savedGroup = await _chatGroupRepository.FindAsync(group.Id);
        savedGroup.ShouldNotBeNull();
        savedGroup.Name.ShouldBe("Test Group");
        savedGroup.Description.ShouldBe("Test Description");
        savedGroup.IsPublic.ShouldBeTrue();
        savedGroup.MaxParticipants.ShouldBe(50);
        savedGroup.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Update_Group_In_Database()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Original Name", "Original Description");

        // Act
        group.UpdateName("Updated Name");
        group.UpdateDescription("Updated Description");
        await _chatGroupRepository.UpdateAsync(group);

        // Assert
        var updatedGroup = await _chatGroupRepository.FindAsync(group.Id);
        updatedGroup.ShouldNotBeNull();
        updatedGroup.Name.ShouldBe("Updated Name");
        updatedGroup.Description.ShouldBe("Updated Description");
    }

    [Fact]
    public async Task Should_Delete_Group_From_Database()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");

        // Act
        await _chatGroupRepository.DeleteAsync(group);

        // Assert
        var deletedGroup = await _chatGroupRepository.FindAsync(group.Id);
        deletedGroup.ShouldBeNull();
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
