using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FamilyChat.Domain.Shared.Enums;

namespace FamilyChat.Application.Services;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<List<T>> GetListAsync<T>(string key);
    Task AddToListAsync<T>(string key, T item, TimeSpan? expiry = null);
    Task RemoveFromListAsync<T>(string key, T item);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedData))
                return default(T);

            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache data for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiry.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiry.Value;
            else
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

            var serializedData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache data for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache data for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            return !string.IsNullOrEmpty(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<List<T>> GetListAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedData))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(cachedData) ?? new List<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached list for key: {Key}", key);
            return new List<T>();
        }
    }

    public async Task AddToListAsync<T>(string key, T item, TimeSpan? expiry = null)
    {
        try
        {
            var list = await GetListAsync<T>(key);
            list.Add(item);
            await SetAsync(key, list, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cached list for key: {Key}", key);
        }
    }

    public async Task RemoveFromListAsync<T>(string key, T item)
    {
        try
        {
            var list = await GetListAsync<T>(key);
            list.Remove(item);
            await SetAsync(key, list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cached list for key: {Key}", key);
        }
    }
}

// Cache keys constants
public static class CacheKeys
{
    public const string CHAT_GROUPS_PREFIX = "chat:groups:";
    public const string CHAT_MESSAGES_PREFIX = "chat:messages:";
    public const string ONLINE_USERS_PREFIX = "users:online:";
    public const string VIDEO_CALL_PREFIX = "call:video:";
    public const string USER_SESSION_PREFIX = "session:user:";
    public const string GROUP_MEMBERS_PREFIX = "group:members:";

    public static string ChatGroups(Guid groupId) => $"{CHAT_GROUPS_PREFIX}{groupId}";
    public static string ChatMessages(Guid groupId, int page = 1) => $"{CHAT_MESSAGES_PREFIX}{groupId}:page:{page}";
    public static string OnlineUsers(Guid groupId) => $"{ONLINE_USERS_PREFIX}{groupId}";
    public static string VideoCall(Guid callId) => $"{VIDEO_CALL_PREFIX}{callId}";
    public static string UserSession(Guid userId) => $"{USER_SESSION_PREFIX}{userId}";
    public static string GroupMembers(Guid groupId) => $"{GROUP_MEMBERS_PREFIX}{groupId}";
}
