using AutoMapper;
using Microsoft.Extensions.Logging;
using SimpleConnect.Application.Contracts.DTOs;
using SimpleConnect.Application.Contracts.Services;
using SimpleConnect.Domain.Entities;
using SimpleConnect.Domain.Repositories;

namespace SimpleConnect.Application.Services;

public class ChatAppService : IChatAppService
{
    private readonly ILogger<ChatAppService> _logger;
    private readonly IMapper _mapper;
    private readonly IChatGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatAppService(
        ILogger<ChatAppService> logger,
        IMapper mapper,
        IChatGroupRepository groupRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _mapper = mapper;
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatGroupDto> CreateGroupAsync(CreateChatGroupDto input)
    {
        _logger.LogInformation("Creating new chat group: {GroupName}", input.Name);

        var group = new ChatGroup(input.Name, input.Description, input.Type, input.CreatorId, input.MaxParticipants);

        await _groupRepository.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<ChatGroupDto>(group);
        return result;
    }

    public async Task<ChatGroupDto> UpdateGroupAsync(Guid groupId, UpdateChatGroupDto input)
    {
        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        group.UpdateName(input.Name);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<ChatGroupDto>(group);
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

        _logger.LogInformation("Group {GroupId} deleted", groupId);
    }

    public async Task<ChatGroupDto> GetGroupAsync(Guid groupId)
    {
        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        var result = _mapper.Map<ChatGroupDto>(group);
        return result;
    }

    public async Task<List<ChatGroupDto>> GetUserGroupsAsync(Guid userId)
    {
        var groups = await _groupRepository.GetUserGroupsAsync(userId);
        var result = _mapper.Map<List<ChatGroupDto>>(groups);
        return result;
    }

    public async Task<ChatGroupDto> JoinGroupAsync(JoinGroupDto input)
    {
        var group = await _groupRepository.GetAsync(input.GroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.GroupId));

        group.AddMember(input.UserId, input.UserName);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<ChatGroupDto>(group);
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

        _logger.LogInformation("User {UserId} left group {GroupId}", input.UserId, input.GroupId);
    }

    public async Task<List<ChatGroupMemberDto>> GetGroupMembersAsync(Guid groupId)
    {
        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        var result = _mapper.Map<List<ChatGroupMemberDto>>(group.Members);
        return result;
    }
}
