using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;
using afonsoft.FamilyMeet.Chat;
using afonsoft.FamilyMeet.Chat.Dtos;

namespace afonsoft.FamilyMeet.Application.Tests.Services.Chat;

public class ChatMessageAppServiceTests : FamilyMeetApplicationTestBase<FamilyMeetApplicationTestModule>
{
    private readonly IChatMessageAppService _chatMessageAppService;

    public ChatMessageAppServiceTests()
    {
        _chatMessageAppService = GetRequiredService<IChatMessageAppService>();
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Paged_Messages()
    {
        // Arrange
        var input = new PagedAndSortedResultRequestDto();

        // Act
        var result = await _chatMessageAppService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAsync_Should_Return_Message_By_Id()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // First create a message
        var createInput = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Test Message",
            Type = MessageType.Text
        };

        var createdMessage = await _chatMessageAppService.CreateAsync(createInput);

        // Act
        var result = await _chatMessageAppService.GetAsync(createdMessage.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdMessage.Id);
        result.Content.ShouldBe("Test Message");
        result.Type.ShouldBe(MessageType.Text);
        result.ChatGroupId.ShouldBe(groupId);
    }

    [Fact]
    public async Task CreateAsync_Should_Create_New_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var input = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "New Test Message",
            Type = MessageType.Text,
            ReplyToMessageId = null
        };

        // Act
        var result = await _chatMessageAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Content.ShouldBe("New Test Message");
        result.Type.ShouldBe(MessageType.Text);
        result.ChatGroupId.ShouldBe(groupId);
        result.IsDeleted.ShouldBeFalse();
        result.CreationTime.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Create a message first
        var createInput = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Original Message",
            Type = MessageType.Text
        };

        var createdMessage = await _chatMessageAppService.CreateAsync(createInput);

        var updateInput = new UpdateChatMessageDto
        {
            Content = "Updated Message"
        };

        // Act
        var result = await _chatMessageAppService.UpdateAsync(createdMessage.Id, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(createdMessage.Id);
        result.Content.ShouldBe("Updated Message");
        result.IsEdited.ShouldBeTrue();
        result.EditedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_Should_Soft_Delete_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Create a message first
        var createInput = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Message to Delete",
            Type = MessageType.Text
        };

        var createdMessage = await _chatMessageAppService.CreateAsync(createInput);

        // Act
        await _chatMessageAppService.DeleteAsync(createdMessage.Id);

        // Assert
        var deletedMessage = await _chatMessageAppService.GetAsync(createdMessage.Id);
        deletedMessage.ShouldNotBeNull();
        deletedMessage.IsDeleted.ShouldBeTrue();
        deletedMessage.DeletedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetMessagesByGroupAsync_Should_Return_Group_Messages()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Create multiple messages for the group
        var message1 = await _chatMessageAppService.CreateAsync(new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Message 1",
            Type = MessageType.Text
        });

        var message2 = await _chatMessageAppService.CreateAsync(new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Message 2",
            Type = MessageType.Text
        });

        var input = new PagedAndSortedResultRequestDto();

        // Act
        var result = await _chatMessageAppService.GetMessagesByGroupAsync(groupId, input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        result.Items.ShouldContain(m => m.Content == "Message 1");
        result.Items.ShouldContain(m => m.Content == "Message 2");
    }

    [Fact]
    public async Task SendAsync_Should_Create_And_Send_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var input = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Sent Message",
            Type = MessageType.Text
        };

        // Act
        var result = await _chatMessageAppService.SendAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Content.ShouldBe("Sent Message");
        result.Type.ShouldBe(MessageType.Text);
        result.ChatGroupId.ShouldBe(groupId);
        result.SentAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateAsync_With_Reply_Should_Create_Reply_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Create original message
        var originalMessage = await _chatMessageAppService.CreateAsync(new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Original Message",
            Type = MessageType.Text
        });

        var replyInput = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Reply Message",
            Type = MessageType.Text,
            ReplyToMessageId = originalMessage.Id
        };

        // Act
        var result = await _chatMessageAppService.CreateAsync(replyInput);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe("Reply Message");
        result.ReplyToMessageId.ShouldBe(originalMessage.Id);
        result.ReplyToMessage.ShouldNotBeNull();
        result.ReplyToMessage.Content.ShouldBe("Original Message");
    }

    [Fact]
    public async Task CreateAsync_With_Image_Type_Should_Create_Image_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var input = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "image.jpg",
            Type = MessageType.Image
        };

        // Act
        var result = await _chatMessageAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe("image.jpg");
        result.Type.ShouldBe(MessageType.Image);
    }

    [Fact]
    public async Task CreateAsync_With_File_Type_Should_Create_File_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var input = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "document.pdf",
            Type = MessageType.File
        };

        // Act
        var result = await _chatMessageAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe("document.pdf");
        result.Type.ShouldBe(MessageType.File);
    }

    [Fact]
    public async Task CreateAsync_With_System_Type_Should_Create_System_Message()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var input = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "User joined the group",
            Type = MessageType.System
        };

        // Act
        var result = await _chatMessageAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Content.ShouldBe("User joined the group");
        result.Type.ShouldBe(MessageType.System);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_Exception_When_Message_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateInput = new UpdateChatMessageDto
        {
            Content = "Updated Content"
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatMessageAppService.UpdateAsync(nonExistentId, updateInput));
    }

    [Fact]
    public async Task DeleteAsync_Should_Throw_Exception_When_Message_Not_Found()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(
            () => _chatMessageAppService.DeleteAsync(nonExistentId));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_Content_Length()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var longContent = new string('A', 4001); // Exceeds max length
        var input = new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = longContent,
            Type = MessageType.Text
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatMessageAppService.CreateAsync(input));
    }

    [Fact]
    public async Task CreateAsync_Should_Validate_ChatGroupId()
    {
        // Arrange
        var invalidGroupId = Guid.Empty;
        var input = new CreateChatMessageDto
        {
            ChatGroupId = invalidGroupId,
            Content = "Test Message",
            Type = MessageType.Text
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatMessageAppService.CreateAsync(input));
    }

    [Fact]
    public async Task UpdateAsync_Should_Validate_Content_Length()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        // Create a message first
        var createdMessage = await _chatMessageAppService.CreateAsync(new CreateChatMessageDto
        {
            ChatGroupId = groupId,
            Content = "Original Message",
            Type = MessageType.Text
        });

        var longContent = new string('A', 4001); // Exceeds max length
        var updateInput = new UpdateChatMessageDto
        {
            Content = longContent
        };

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Validation.AbpValidationException>(
            () => _chatMessageAppService.UpdateAsync(createdMessage.Id, updateInput));
    }
}
