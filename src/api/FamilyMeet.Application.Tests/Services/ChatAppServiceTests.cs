using Microsoft.Extensions.Logging;
using AutoMapper;
using Moq;
using Shouldly;
using Xunit;
using FamilyMeet.Application.Services;
using FamilyMeet.Application.Contracts.Services;
using FamilyMeet.Application.Contracts.DTOs;
using FamilyMeet.Domain.Entities;
using FamilyMeet.Domain.Repositories;
using FamilyMeet.Application.Tests;

namespace FamilyMeet.Application.Tests.Services;

public class ChatAppServiceTests : FamilyMeetApplicationTestBase
{
    private readonly Mock<ILogger<ChatAppService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IChatGroupRepository> _groupRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRedisCacheService> _cacheServiceMock;
    private readonly ChatAppService _chatAppService;

    public ChatAppServiceTests()
    {
        _loggerMock = CreateMock<ILogger<ChatAppService>>();
        _mapperMock = CreateMock<IMapper>();
        _groupRepositoryMock = CreateMock<IChatGroupRepository>();
        _unitOfWorkMock = CreateMock<IUnitOfWork>();
        _cacheServiceMock = CreateMock<IRedisCacheService>();

        _chatAppService = new ChatAppService(
            _loggerMock.Object,
            _mapperMock.Object,
            _groupRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object
        );
    }

    [Fact]
    public async Task CreateGroupAsync_ShouldCreateGroupAndCacheResult()
    {
        // Arrange
        var input = new CreateChatGroupDto
        {
            Name = "Test Group",
            Description = "Test Description",
            Type = "chat",
            CreatorId = Guid.NewGuid(),
            MaxParticipants = 50
        };

        var expectedGroup = new ChatGroup(input.Name, input.Description, input.Type, input.CreatorId, input.MaxParticipants);
        var expectedDto = new FamilyMeetGroupDto
        {
            Id = expectedGroup.Id,
            Name = input.Name,
            Description = input.Description,
            Type = input.Type,
            CreatorId = input.CreatorId,
            MaxParticipants = input.MaxParticipants,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _groupRepositoryMock.Setup(x => x.AddAsync(It.IsAny<ChatGroup>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(1));
        _mapperMock.Setup(x => x.Map<FamilyMeetGroupDto>(It.IsAny<ChatGroup>())).Returns(expectedDto);
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), expectedDto, It.IsAny<TimeSpan>())).Returns(Task.CompletedTask);

        // Act
        var result = await _chatAppService.CreateGroupAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);
        result.Description.ShouldBe(input.Description);
        result.Type.ShouldBe(input.Type);
        result.CreatorId.ShouldBe(input.CreatorId);
        result.MaxParticipants.ShouldBe(input.MaxParticipants);

        _groupRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ChatGroup>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mapperMock.Verify(x => x.Map<FamilyMeetGroupDto>(It.IsAny<ChatGroup>()), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), expectedDto, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetGroupAsync_ShouldReturnFromCache_WhenGroupIsCached()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var cachedGroup = new FamilyMeetGroupDto
        {
            Id = groupId,
            Name = "Cached Group",
            Description = "Cached Description",
            Type = "chat",
            CreatorId = Guid.NewGuid(),
            MaxParticipants = 50,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _cacheServiceMock.Setup(x => x.GetAsync<FamilyMeetGroupDto>(It.IsAny<string>())).ReturnsAsync(cachedGroup);

        // Act
        var result = await _chatAppService.GetGroupAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(groupId);
        result.Name.ShouldBe(cachedGroup.Name);

        _cacheServiceMock.Verify(x => x.GetAsync<FamilyMeetGroupDto>(It.IsAny<string>()), Times.Once);
        _groupRepositoryMock.Verify(x => x.GetAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetGroupAsync_ShouldReturnFromRepository_WhenGroupIsNotCached()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = new ChatGroup("Test Group", "Test Description", "chat", Guid.NewGuid(), 50);
        var expectedDto = new FamilyMeetGroupDto
        {
            Id = groupId,
            Name = "Test Group",
            Description = "Test Description",
            Type = "chat",
            CreatorId = Guid.NewGuid(),
            MaxParticipants = 50,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _cacheServiceMock.Setup(x => x.GetAsync<FamilyMeetGroupDto>(It.IsAny<string>())).ReturnsAsync((FamilyMeetGroupDto?)null);
        _groupRepositoryMock.Setup(x => x.GetAsync(groupId)).ReturnsAsync(group);
        _mapperMock.Setup(x => x.Map<FamilyMeetGroupDto>(group)).Returns(expectedDto);
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), expectedDto, It.IsAny<TimeSpan>())).Returns(Task.CompletedTask);

        // Act
        var result = await _chatAppService.GetGroupAsync(groupId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(groupId);
        result.Name.ShouldBe(expectedDto.Name);

        _cacheServiceMock.Verify(x => x.GetAsync<FamilyMeetGroupDto>(It.IsAny<string>()), Times.Once);
        _groupRepositoryMock.Verify(x => x.GetAsync(groupId), Times.Once);
        _mapperMock.Verify(x => x.Map<FamilyMeetGroupDto>(group), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), expectedDto, It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetGroupAsync_ShouldThrowException_WhenGroupNotFound()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        _cacheServiceMock.Setup(x => x.GetAsync<FamilyMeetGroupDto>(It.IsAny<string>())).ReturnsAsync((FamilyMeetGroupDto?)null);
        _groupRepositoryMock.Setup(x => x.GetAsync(groupId)).ReturnsAsync((ChatGroup?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () => await _chatAppService.GetGroupAsync(groupId));
        exception.Message.ShouldContain("Group not found");
    }

    [Fact]
    public async Task DeleteGroupAsync_ShouldDeactivateGroupAndUpdateCache()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var group = new ChatGroup("Test Group", "Test Description", "chat", Guid.NewGuid(), 50);

        _groupRepositoryMock.Setup(x => x.GetAsync(groupId)).ReturnsAsync(group);
        _groupRepositoryMock.Setup(x => x.UpdateAsync(group)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(1));
        _cacheServiceMock.Setup(x => x.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await _chatAppService.DeleteGroupAsync(groupId);

        // Assert
        group.IsActive.ShouldBeFalse();
        
        _groupRepositoryMock.Verify(x => x.GetAsync(groupId), Times.Once);
        _groupRepositoryMock.Verify(x => x.UpdateAsync(group), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Exactly(2)); // ChatGroups and GroupMembers
    }

    [Fact]
    public async Task JoinGroupAsync_ShouldAddMemberAndUpdateCache()
    {
        // Arrange
        var input = new JoinGroupDto
        {
            GroupId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserName = "Test User"
        };

        var group = new ChatGroup("Test Group", "Test Description", "chat", Guid.NewGuid(), 50);
        var expectedDto = new FamilyMeetGroupDto
        {
            Id = input.GroupId,
            Name = "Test Group",
            Description = "Test Description",
            Type = "chat",
            CreatorId = Guid.NewGuid(),
            MaxParticipants = 50,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _groupRepositoryMock.Setup(x => x.GetAsync(input.GroupId)).ReturnsAsync(group);
        _groupRepositoryMock.Setup(x => x.UpdateAsync(group)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(1));
        _mapperMock.Setup(x => x.Map<FamilyMeetGroupDto>(group)).Returns(expectedDto);
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), expectedDto, It.IsAny<TimeSpan>())).Returns(Task.CompletedTask);
        _cacheServiceMock.Setup(x => x.AddToListAsync(It.IsAny<string>(), input.UserId, It.IsAny<TimeSpan>())).Returns(Task.CompletedTask);

        // Act
        var result = await _chatAppService.JoinGroupAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(input.GroupId);

        group.Members.ShouldContain(m => m.UserId == input.UserId && m.UserName == input.UserName);

        _groupRepositoryMock.Verify(x => x.GetAsync(input.GroupId), Times.Once);
        _groupRepositoryMock.Verify(x => x.UpdateAsync(group), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mapperMock.Verify(x => x.Map<FamilyMeetGroupDto>(group), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), expectedDto, It.IsAny<TimeSpan>()), Times.Once);
        _cacheServiceMock.Verify(x => x.AddToListAsync(It.IsAny<string>(), input.UserId, It.IsAny<TimeSpan>()), Times.Once);
    }
}
