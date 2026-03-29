using Microsoft.EntityFrameworkCore;
using FamiyChat.Domain.Entities;
using FamiyChat.Domain.Repositories;
using FamiyChat.EntityFrameworkCore;

namespace FamiyChat.EntityFrameworkCore.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly FamiyChatDbContext _context;

    public ChatMessageRepository(FamiyChatDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage?> GetAsync(Guid id)
    {
        return await _context.ChatMessages
            .Include(x => x.Attachments)
            .Include(x => x.ReplyToMessage)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(List<ChatMessage> messages, int totalCount)> GetMessagesAsync(
        Guid groupId,
        int pageNumber,
        int pageSize,
        DateTime? beforeDate = null,
        DateTime? afterDate = null)
    {
        var query = _context.ChatMessages
            .Include(x => x.Attachments)
            .Include(x => x.ReplyToMessage)
            .Where(x => x.ChatGroupId == groupId && !x.IsDeleted);

        if (beforeDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt < beforeDate.Value);
        }

        if (afterDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt > afterDate.Value);
        }

        var totalCount = await query.CountAsync();

        var messages = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (messages, totalCount);
    }

    public async Task AddAsync(ChatMessage message)
    {
        await _context.ChatMessages.AddAsync(message);
    }

    public Task UpdateAsync(ChatMessage message)
    {
        _context.ChatMessages.Update(message);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ChatMessage message)
    {
        _context.ChatMessages.Remove(message);
        return Task.CompletedTask;
    }
}
