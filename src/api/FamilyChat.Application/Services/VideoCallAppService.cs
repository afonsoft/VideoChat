using AutoMapper;
using Microsoft.Extensions.Logging;
using FamiyChat.Application.Contracts.DTOs;
using FamiyChat.Application.Contracts.Services;
using FamiyChat.Domain.Entities;
using FamiyChat.Domain.Shared.ValueObjects;
using FamiyChat.Domain.Shared.Enums;
using FamiyChat.Domain.Repositories;

namespace FamiyChat.Application.Services;

public class VideoCallAppService : IVideoCallAppService
{
    private readonly ILogger<VideoCallAppService> _logger;
    private readonly IMapper _mapper;
    private readonly IChatGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VideoCallAppService(
        ILogger<VideoCallAppService> logger,
        IMapper mapper,
        IChatGroupRepository groupRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _mapper = mapper;
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CallInfoDto> JoinCallAsync(JoinCallDto input)
    {
        var group = await _groupRepository.GetAsync(input.GroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.GroupId));

        if (!group.IsMember(input.UserId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        if (!group.CanUserJoinCall(input.UserId))
            throw new InvalidOperationException("User cannot join the call");

        var participant = new CallParticipant(input.UserId, input.UserName, ParticipantStatus.Connected)
            .WithAudio(input.HasAudio)
            .WithVideo(input.HasVideo);

        group.AddCallParticipant(participant);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        var result = new CallInfoDto
        {
            GroupId = group.Id,
            GroupName = group.Name,
            Participants = _mapper.Map<List<CallParticipantDto>>(group.ActiveCallParticipants),
            IsActive = true,
            StartedAt = DateTime.UtcNow,
            MaxParticipants = group.MaxParticipants
        };

        _logger.LogInformation("User {UserId} joined call in group {GroupId}", input.UserId, input.GroupId);
        return result;
    }

    public async Task LeaveCallAsync(LeaveCallDto input)
    {
        var group = await _groupRepository.GetAsync(input.GroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.GroupId));

        group.RemoveCallParticipant(input.UserId);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserId} left call in group {GroupId}", input.UserId, input.GroupId);
    }

    public async Task<CallInfoDto> GetCallInfoAsync(Guid groupId)
    {
        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        var result = new CallInfoDto
        {
            GroupId = group.Id,
            GroupName = group.Name,
            Participants = _mapper.Map<List<CallParticipantDto>>(group.ActiveCallParticipants),
            IsActive = group.ActiveCallParticipants.Any(),
            StartedAt = group.ActiveCallParticipants.Any() ? group.ActiveCallParticipants.Min(p => p.JoinedAt) : DateTime.UtcNow,
            MaxParticipants = group.MaxParticipants
        };

        return result;
    }

    public async Task UpdateParticipantStatusAsync(UpdateParticipantStatusDto input)
    {
        var group = await _groupRepository.GetAsync(input.GroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.GroupId));

        var participant = group.ActiveCallParticipants.FirstOrDefault(p => p.UserId == input.UserId);
        if (participant == null)
            throw new ArgumentException("User is not in the call", nameof(input.UserId));

        var updatedParticipant = participant
            .WithStatus(input.Status)
            .WithAudio(input.HasAudio)
            .WithVideo(input.HasVideo);

        group.RemoveCallParticipant(input.UserId);
        group.AddCallParticipant(updatedParticipant);

        await _groupRepository.UpdateAsync(group);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated status in group {GroupId}", input.UserId, input.GroupId);
    }

    public async Task<List<CallParticipantDto>> GetCallParticipantsAsync(Guid groupId)
    {
        var group = await _groupRepository.GetAsync(groupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(groupId));

        var result = _mapper.Map<List<CallParticipantDto>>(group.ActiveCallParticipants);
        return result;
    }
}
