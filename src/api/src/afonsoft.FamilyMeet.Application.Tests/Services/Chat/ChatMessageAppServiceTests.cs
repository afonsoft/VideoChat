using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Application.Chat;

public abstract class ChatMessageAppServiceTests<TModule> : FamilyMeetApplicationTestBase<TModule>
    where TModule : IAbpModule
{
    private readonly IChatMessageAppService _chatMessageAppService;
    private readonly IRepository<ChatMessage, Guid> _chatMessageRepository;
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository;

    protected ChatMessageAppServiceTests()
    {
        _chatMessageAppService = GetRequiredService<IChatMessageAppService>();
        _chatMessageRepository = GetRequiredService<IRepository<ChatMessage, Guid>>();
        _chatGroupRepository = GetRequiredService<IRepository<ChatGroup, Guid>>();
    }

    [Fact]
    public async Task Should_Create_Message()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var input = new CreateChatMessageDto
        {
            ChatGroupId = group.Id,
            Content = "Test Message",
            Type = MessageType.Text
        };

        // Act
        var result = await _chatMessageAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.ChatGroupId.ShouldBe(input.ChatGroupId);
        result.Content.ShouldBe(input.Content);
        result.Type.ShouldBe(input.Type);
        result.IsEdited.ShouldBeFalse();
        result.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Send_Message()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var input = new CreateChatMessageDto
        {
            ChatGroupId = group.Id,
            Content = "Test Message",
            Type = MessageType.Text
        };

        // Act
        var result = await _chatMessageAppService.SendAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe(input.Content);
        result.Type.ShouldBe(input.Type);
    }

    [Fact]
    public async Task Should_Get_Messages_By_Group()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        await CreateTestMessageAsync(group.Id, "Message 1");
        await CreateTestMessageAsync(group.Id, "Message 2");

        // Act
        var result = await _chatMessageAppService.GetListAsync(
            new ChatMessageListDto { ChatGroupId = group.Id });

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.ShouldAllBe(x => x.ChatGroupId == group.Id);
    }

    [Fact]
    public async Task Should_Filter_Messages_By_Type()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        await CreateTestMessageAsync(group.Id, "Text Message", MessageType.Text);
        await CreateTestMessageAsync(group.Id, "System Message", MessageType.System);

        // Act
        var result = await _chatMessageAppService.GetListAsync(
            new ChatMessageListDto 
            { 
                ChatGroupId = group.Id,
                Type = MessageType.Text
            });

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        result.Items.ShouldAllBe(x => x.Type == MessageType.Text);
    }

    [Fact]
    public async Task Should_Edit_Message()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var message = await CreateTestMessageAsync(group.Id, "Original Content");
        var input = new UpdateChatMessageDto
        {
            Content = "Updated Content"
        };

        // Act
        var result = await _chatMessageAppService.EditAsync(message.Id, input);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe(input.Content);
        result.IsEdited.ShouldBeTrue();
        result.EditedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Soft_Delete_Message()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var message = await CreateTestMessageAsync(group.Id, "Test Message");

        // Act
        await _chatMessageAppService.DeleteSoftAsync(message.Id);

        // Assert
        var deletedMessage = await _chatMessageRepository.FindAsync(message.Id);
        deletedMessage.ShouldNotBeNull();
        deletedMessage.IsDeleted.ShouldBeTrue();
        deletedMessage.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Handle_Reply_To_Message()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var parentMessage = await CreateTestMessageAsync(group.Id, "Parent Message");
        var input = new CreateChatMessageDto
        {
            ChatGroupId = group.Id,
            Content = "Reply Message",
            Type = MessageType.Text,
            ReplyToMessageId = parentMessage.Id
        };

        // Act
        var result = await _chatMessageAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.ReplyToMessageId.ShouldBe(parentMessage.Id);
    }

    [Fact]
    public async Task Should_Handle_Different_Message_Types()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");

        // Act
        var textMessage = await CreateTestMessageAsync(group.Id, "Text Message", MessageType.Text);
        var imageMessage = await CreateTestMessageAsync(group.Id, "Image Message", MessageType.Image);
        var fileMessage = await CreateTestMessageAsync(group.Id, "File Message", MessageType.File);
        var systemMessage = await CreateTestMessageAsync(group.Id, "System Message", MessageType.System);

        // Assert
        textMessage.Type.ShouldBe(MessageType.Text);
        imageMessage.Type.ShouldBe(MessageType.Image);
        fileMessage.Type.ShouldBe(MessageType.File);
        systemMessage.Type.ShouldBe(MessageType.System);
    }

    [Fact]
    public async Task Should_Get_Only_Active_Messages()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var activeMessage = await CreateTestMessageAsync(group.Id, "Active Message");
        var deletedMessage = await CreateTestMessageAsync(group.Id, "Deleted Message");
        
        // Soft delete one message
        deletedMessage.Delete();
        await _chatMessageRepository.UpdateAsync(deletedMessage);

        // Act
        var result = await _chatMessageAppService.GetListAsync(
            new ChatMessageListDto 
            { 
                ChatGroupId = group.Id,
                IncludeDeleted = false
            });

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.ShouldAllBe(x => !x.IsDeleted);
    }

    [Fact]
    public async Task Should_Handle_Content_Length_Validation()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var longContent = new string('A', ChatConstants.ChatMessage.MaxContentLength + 1);
        var input = new CreateChatMessageDto
        {
            ChatGroupId = group.Id,
            Content = longContent,
            Type = MessageType.Text
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatMessageAppService.CreateAsync(input));
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

    private async Task<ChatMessage> CreateTestMessageAsync(
        Guid groupId, 
        string content, 
        MessageType type = MessageType.Text,
        Guid? replyToMessageId = null)
    {
        var message = new ChatMessage(
            GuidGenerator.Create(),
            groupId,
            Guid.NewGuid(), // Simulate user ID
            "Test User",
            content,
            type,
            replyToMessageId
        );

        return await _chatMessageRepository.InsertAsync(message);
    }
}
