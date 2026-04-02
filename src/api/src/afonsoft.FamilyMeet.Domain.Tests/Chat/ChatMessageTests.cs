using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Volo.Abp.Domain.Repositories;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Localization;

namespace afonsoft.FamilyMeet.Domain.Chat;

public abstract class ChatMessageTests<TModule> : FamilyMeetDomainTestBase<TModule>
    where TModule : IAbpModule
{
    private readonly IRepository<ChatMessage, Guid> _chatMessageRepository;
    private readonly IRepository<ChatGroup, Guid> _chatGroupRepository;

    protected ChatMessageTests()
    {
        _chatMessageRepository = GetRequiredService<IRepository<ChatMessage, Guid>>();
        _chatGroupRepository = GetRequiredService<IRepository<ChatGroup, Guid>>();
    }

    [Fact]
    public async Task Should_Create_Message_With_Valid_Data()
    {
        // Arrange
        var groupId = GuidGenerator.Create();
        var senderId = GuidGenerator.Create();
        var senderName = "Test User";
        var content = "Test Message";
        var type = MessageType.Text;

        // Act
        var message = new ChatMessage(
            GuidGenerator.Create(),
            groupId,
            senderId,
            senderName,
            content,
            type
        );

        // Assert
        message.ShouldNotBeNull();
        message.ChatGroupId.ShouldBe(groupId);
        message.SenderId.ShouldBe(senderId);
        message.SenderName.ShouldBe(senderName);
        message.Content.ShouldBe(content);
        message.Type.ShouldBe(type);
        message.IsEdited.ShouldBeFalse();
        message.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Edit_Message_Content()
    {
        // Arrange
        var message = await CreateTestMessageAsync("Original Content");

        // Act
        message.EditContent("Updated Content");

        // Assert
        message.Content.ShouldBe("Updated Content");
        message.IsEdited.ShouldBeTrue();
        message.EditedAt.ShouldNotBeNull();
        message.EditedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public async Task Should_Delete_Message()
    {
        // Arrange
        var message = await CreateTestMessageAsync("Test Message");

        // Act
        message.Delete();

        // Assert
        message.IsDeleted.ShouldBeTrue();
        message.DeletedAt.ShouldNotBeNull();
        message.DeletedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public async Task Should_Restore_Deleted_Message()
    {
        // Arrange
        var message = await CreateTestMessageAsync("Test Message");
        message.Delete();

        // Act
        message.Restore();

        // Assert
        message.IsDeleted.ShouldBeFalse();
        message.DeletedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Handle_Reply_To_Message()
    {
        // Arrange
        var parentMessage = await CreateTestMessageAsync("Parent Message");
        var replyMessage = new ChatMessage(
            GuidGenerator.Create(),
            parentMessage.ChatGroupId,
            GuidGenerator.Create(),
            "Reply User",
            "Reply Message",
            MessageType.Text,
            parentMessage.Id
        );

        // Assert
        replyMessage.ReplyToMessageId.ShouldBe(parentMessage.Id);
    }

    [Fact]
    public async Task Should_Handle_Different_Message_Types()
    {
        // Arrange & Act
        var textMessage = new ChatMessage(
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            "User",
            "Text Message",
            MessageType.Text
        );

        var imageMessage = new ChatMessage(
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            "User",
            "Image Message",
            MessageType.Image
        );

        var fileMessage = new ChatMessage(
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            "User",
            "File Message",
            MessageType.File
        );

        var systemMessage = new ChatMessage(
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            GuidGenerator.Create(),
            "System",
            "System Message",
            MessageType.System
        );

        // Assert
        textMessage.Type.ShouldBe(MessageType.Text);
        imageMessage.Type.ShouldBe(MessageType.Image);
        fileMessage.Type.ShouldBe(MessageType.File);
        systemMessage.Type.ShouldBe(MessageType.System);
    }

    [Fact]
    public async Task Should_Persist_Message_To_Database()
    {
        // Arrange
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        var message = new ChatMessage(
            GuidGenerator.Create(),
            group.Id,
            GuidGenerator.Create(),
            "Test User",
            "Test Message",
            MessageType.Text
        );

        // Act
        await _chatMessageRepository.InsertAsync(message);

        // Assert
        var savedMessage = await _chatMessageRepository.FindAsync(message.Id);
        savedMessage.ShouldNotBeNull();
        savedMessage.ChatGroupId.ShouldBe(group.Id);
        savedMessage.Content.ShouldBe("Test Message");
        savedMessage.Type.ShouldBe(MessageType.Text);
        savedMessage.IsEdited.ShouldBeFalse();
        savedMessage.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Update_Message_In_Database()
    {
        // Arrange
        var message = await CreateTestMessageAsync("Original Content");

        // Act
        message.EditContent("Updated Content");
        await _chatMessageRepository.UpdateAsync(message);

        // Assert
        var updatedMessage = await _chatMessageRepository.FindAsync(message.Id);
        updatedMessage.ShouldNotBeNull();
        updatedMessage.Content.ShouldBe("Updated Content");
        updatedMessage.IsEdited.ShouldBeTrue();
        updatedMessage.EditedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Soft_Delete_Message_In_Database()
    {
        // Arrange
        var message = await CreateTestMessageAsync("Test Message");

        // Act
        message.Delete();
        await _chatMessageRepository.UpdateAsync(message);

        // Assert
        var deletedMessage = await _chatMessageRepository.FindAsync(message.Id);
        deletedMessage.ShouldNotBeNull();
        deletedMessage.IsDeleted.ShouldBeTrue();
        deletedMessage.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Handle_Content_Length_Validation()
    {
        // Arrange
        var groupId = GuidGenerator.Create();
        var senderId = GuidGenerator.Create();
        var senderName = "Test User";
        var validContent = new string('A', ChatConstants.ChatMessage.MaxContentLength);

        // Act
        var message = new ChatMessage(
            GuidGenerator.Create(),
            groupId,
            senderId,
            senderName,
            validContent,
            MessageType.Text
        );

        // Assert
        message.Content.ShouldBe(validContent);
        message.Content.Length.ShouldBe(ChatConstants.ChatMessage.MaxContentLength);
    }

    [Fact]
    public async Task Should_Handle_Sender_Name_Length_Validation()
    {
        // Arrange
        var groupId = GuidGenerator.Create();
        var senderId = GuidGenerator.Create();
        var validSenderName = new string('A', ChatConstants.ChatMessage.MaxSenderNameLength);

        // Act
        var message = new ChatMessage(
            GuidGenerator.Create(),
            groupId,
            senderId,
            validSenderName,
            "Test Message",
            MessageType.Text
        );

        // Assert
        message.SenderName.ShouldBe(validSenderName);
        message.SenderName.Length.ShouldBe(ChatConstants.ChatMessage.MaxSenderNameLength);
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
        string content, 
        MessageType type = MessageType.Text,
        Guid? replyToMessageId = null)
    {
        var group = await CreateTestGroupAsync("Test Group", "Test Description");
        
        var message = new ChatMessage(
            GuidGenerator.Create(),
            group.Id,
            GuidGenerator.Create(),
            "Test User",
            content,
            type,
            replyToMessageId
        );

        return await _chatMessageRepository.InsertAsync(message);
    }
}
