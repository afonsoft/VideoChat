using Microsoft.EntityFrameworkCore;
using FamilyChat.Domain.Entities;
using FamilyChat.Domain.Repositories;
using FamilyChat.EntityFrameworkCore;

namespace FamilyChat.EntityFrameworkCore.Repositories;

public class ChatGroupRepository : IChatGroupRepository
{
    private readonly FamilyChatDbContext _context;

    public ChatGroupRepository(FamilyChatDbContext context)
    {
        _context = context;
    }

    public async Task<ChatGroup?> GetAsync(Guid id)
    {
        return await _context.ChatGroups
            .Include(x => x.Members)
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ChatGroup>> GetUserGroupsAsync(Guid userId)
    {
        return await _context.ChatGroups
            .Include(x => x.Members)
            .Where(x => x.Members.Any(m => m.UserId == userId && m.IsActive))
            .OrderByDescending(x => x.LastActivityAt)
            .ToListAsync();
    }

    public async Task AddAsync(ChatGroup group)
    {
        await _context.ChatGroups.AddAsync(group);
    }

    public Task UpdateAsync(ChatGroup group)
    {
        _context.ChatGroups.Update(group);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ChatGroup group)
    {
        _context.ChatGroups.Remove(group);
        return Task.CompletedTask;
    }
}
