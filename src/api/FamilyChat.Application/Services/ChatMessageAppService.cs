using AutoMapper;
using Microsoft.Extensions.Logging;
using FamiyChat.Application.Contracts.DTOs;
using FamiyChat.Application.Contracts.Services;
using FamiyChat.Domain.Entities;
using FamiyChat.Domain.Repositories;

namespace FamiyChat.Application.Services;

public class ChatMessageAppService : IChatMessageAppService
{
    private readonly ILogger<ChatMessageAppService> _logger;
    private readonly IMapper _mapper;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatMessageAppService(
        ILogger<ChatMessageAppService> logger,
        IMapper mapper,
        IChatMessageRepository messageRepository,
        IChatGroupRepository groupRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _mapper = mapper;
        _messageRepository = messageRepository;
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto input)
    {
        var group = await _groupRepository.GetAsync(input.ChatGroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.ChatGroupId));

        if (!group.IsMember(input.SenderId))
            throw new UnauthorizedAccessException("User is not a member of this group");

        var message = new ChatMessage(input.Content, input.SenderId, "User", input.ChatGroupId, input.Type);

        if (input.ReplyToMessageId.HasValue)
        {
            message.SetReply(input.ReplyToMessageId.Value);
        }

        await _messageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<ChatMessageDto>(message);
        return result;
    }

    public async Task<ChatMessageDto> EditMessageAsync(Guid messageId, EditMessageDto input)
    {
        var message = await _messageRepository.GetAsync(messageId);
        if (message == null)
            throw new ArgumentException("Message not found", nameof(messageId));

        message.EditContent(input.Content);

        await _messageRepository.UpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<ChatMessageDto>(message);
        return result;
    }

    public async Task DeleteMessageAsync(Guid messageId)
    {
        var message = await _messageRepository.GetAsync(messageId);
        if (message == null)
            throw new ArgumentException("Message not found", nameof(messageId));

        message.Delete();

        await _messageRepository.UpdateAsync(message);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Message {MessageId} deleted", messageId);
    }

    public async Task<MessagePagedResultDto> GetMessagesAsync(GetMessagesDto input)
    {
        var group = await _groupRepository.GetAsync(input.ChatGroupId);
        if (group == null)
            throw new ArgumentException("Group not found", nameof(input.ChatGroupId));

        var (messages, totalCount) = await _messageRepository.GetMessagesAsync(
            input.ChatGroupId,
            input.PageNumber,
            input.PageSize,
            input.BeforeDate,
            input.AfterDate);

        var messageDtos = _mapper.Map<List<ChatMessageDto>>(messages);

        return new MessagePagedResultDto
        {
            Items = messageDtos,
            TotalCount = totalCount,
            PageNumber = input.PageNumber,
            PageSize = input.PageSize,
            HasNextPage = input.PageNumber * input.PageSize < totalCount
        };
    }

    public async Task<ChatMessageDto> GetMessageAsync(Guid messageId)
    {
        var message = await _messageRepository.GetAsync(messageId);
        if (message == null)
            throw new ArgumentException("Message not found", nameof(messageId));

        var result = _mapper.Map<ChatMessageDto>(message);
        return result;
    }
}
