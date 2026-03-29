using FamiyChat.Domain.Entities;

namespace FamiyChat.Domain.Repositories;

public interface IChatGroupRepository
{
    Task<ChatGroup?> GetAsync(Guid id);
    Task<List<ChatGroup>> GetUserGroupsAsync(Guid userId);
    Task AddAsync(ChatGroup group);
    Task UpdateAsync(ChatGroup group);
    Task DeleteAsync(ChatGroup group);
}

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetAsync(Guid id);
    Task<(List<ChatMessage> messages, int totalCount)> GetMessagesAsync(
        Guid groupId,
        int pageNumber,
        int pageSize,
        DateTime? beforeDate = null,
        DateTime? afterDate = null);
    Task AddAsync(ChatMessage message);
    Task UpdateAsync(ChatMessage message);
    Task DeleteAsync(ChatMessage message);
}

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync();
}
