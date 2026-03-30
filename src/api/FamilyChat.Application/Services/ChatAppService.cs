using AutoMapper;
using Microsoft.Extensions.Logging;
using FamilyChat.Application.Contracts.DTOs;
using FamilyChat.Application.Contracts.Services;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Repositories;

namespace FamilyChat.Application.Services;

public class ChatAppService : IChatAppService
{
    private readonly ILogger<ChatAppService> _logger;
    private readonly IMapper _mapper;
    private readonly IChatGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRedisCacheService _cacheService;

    public ChatAppService(
        ILogger<ChatAppService> logger,
        IMapper mapper,
        IChatGroupRepository groupRepository,
        IUnitOfWork unitOfWork,
        IRedisCacheService cacheService)
    {
        _logger = logger;
        _mapper = mapper;
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<FamilyChatGroupDto> CreateGroupAsync(CreateChatGroupDto input)
    {
        _logger.LogInformation("Creating new chat group: {GroupName}", input.Name);

        var group = new ChatGroup(input.Name, input.Description, input.Type, input.CreatorId, input.MaxParticipants);

        await _groupRepository.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();

        // Cache the new group
        var result = _mapper.Map<FamilyChatGroupDto>(group);
        await _cacheService.SetAsync(CacheKeys.ChatGroups(group.Id), result, TimeSpan.FromHours(1));

        return result;
    }

    public async Task<FamilyChatGroupDto> UpdateGroupAsync(Guid groupId, UpdateChatGroupDto input)
    {
        // Try to get from cache first
        var cachedGroup = await _cacheService.GetAsync<FamilyChatGroupDto>(CacheKeys.ChatGroups(groupId));

        var group = cachedGroup != null ?
            await _groupRepository.GetAsync(groupId) :
            await _groupRepository.GetAsync(groupId);

        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        group.UpdateName(input.Name);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        // Update cache
        var result = _mapper.Map<FamilyChatGroupDto>(group);
        await _cacheService.SetAsync(CacheKeys.ChatGroups(group.Id), result, TimeSpan.FromHours(1));

        return result;
    }

    public async Task DeleteGroupAsync(Guid groupId)
    {
        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        group.Deactivate();

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        // Remove from cache
        await _cacheService.RemoveAsync(CacheKeys.ChatGroups(groupId));
        await _cacheService.RemoveAsync(CacheKeys.GroupMembers(groupId));

        _logger.LogInformation("Group {GroupId} deleted", groupId);
    }

    public async Task<FamilyChatGroupDto> GetGroupAsync(Guid groupId)
    {
        // Try to get from cache first
        var cachedGroup = await _cacheService.GetAsync<FamilyChatGroupDto>(CacheKeys.ChatGroups(groupId));
        if (cachedGroup != null)
        {
            _logger.LogDebug("Group {GroupId} retrieved from cache", groupId);
            return cachedGroup;
        }

        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        var result = _mapper.Map<FamilyChatGroupDto>(group);

        // Cache for 1 hour
        await _cacheService.SetAsync(CacheKeys.ChatGroups(group.Id), result, TimeSpan.FromHours(1));

        return result;
    }

    public async Task<List<FamilyChatGroupDto>> GetUserGroupsAsync(Guid userId)
    {
        var cacheKey = $"user:groups:{userId}";

        // Try to get from cache first
        var cachedGroups = await _cacheService.GetAsync<List<FamilyChatGroupDto>>(cacheKey);
        if (cachedGroups != null)
        {
            _logger.LogDebug("User {UserId} groups retrieved from cache", userId);
            return cachedGroups;
        }

        var groups = await _groupRepository.GetUserGroupsAsync(userId);
        var result = _mapper.Map<List<FamilyChatGroupDto>>(groups);

        // Cache for 30 minutes
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<FamilyChatGroupDto> JoinGroupAsync(JoinGroupDto input)
    {
        var group = await _groupRepository.GetAsync(input.GroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.GroupId));

        group.AddMember(input.UserId, input.UserName);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        // Update caches
        var result = _mapper.Map<FamilyChatGroupDto>(group);
        await _cacheService.SetAsync(CacheKeys.ChatGroups(group.Id), result, TimeSpan.FromHours(1));

        // Update online users cache
        await _cacheService.AddToListAsync(CacheKeys.OnlineUsers(group.Id), input.UserId, TimeSpan.FromMinutes(15));

        return result;
    }

    public async Task LeaveGroupAsync(LeaveGroupDto input)
    {
        var group = await _groupRepository.GetAsync(input.GroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.GroupId));

        group.RemoveMember(input.UserId);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        // Update caches
        var result = _mapper.Map<FamilyChatGroupDto>(group);
        await _cacheService.SetAsync(CacheKeys.ChatGroups(group.Id), result, TimeSpan.FromHours(1));

        // Remove from online users cache
        await _cacheService.RemoveFromListAsync<Guid>(CacheKeys.OnlineUsers(group.Id), input.UserId);

        _logger.LogInformation("User {UserId} left group {GroupId}", input.UserId, input.GroupId);
    }

    public async Task<List<ChatGroupMemberDto>> GetGroupMembersAsync(Guid groupId)
    {
        var cacheKey = CacheKeys.GroupMembers(groupId);

        // Try to get from cache first
        var cachedMembers = await _cacheService.GetAsync<List<ChatGroupMemberDto>>(cacheKey);
        if (cachedMembers != null)
        {
            _logger.LogDebug("Group {GroupId} members retrieved from cache", groupId);
            return cachedMembers;
        }

        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        var result = _mapper.Map<List<ChatGroupMemberDto>>(group.Members);

        // Cache for 15 minutes
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

        return result;
    }
}
