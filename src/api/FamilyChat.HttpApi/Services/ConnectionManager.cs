using Microsoft.AspNetCore.SignalR;

namespace FamilyChat.HttpApi.Services;

public interface IConnectionManager
{
    Task AddConnectionAsync(Guid userId, string connectionId);
    Task RemoveConnectionAsync(Guid userId, string connectionId);
    Task<List<string>> GetConnectionsAsync(Guid userId);
    Task<Guid?> GetUserIdByConnectionAsync(string connectionId);
    Task<List<Guid>> GetConnectedUsersAsync();
}

public class ConnectionManager : IConnectionManager
{
    private readonly HubContext<CommunicationHub> _hubContext;
    private readonly Dictionary<Guid, HashSet<string>> _userConnections = new();
    private readonly Dictionary<string, Guid> _connectionUsers = new();
    private readonly object _lock = new object();

    public ConnectionManager(HubContext<CommunicationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task AddConnectionAsync(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_userConnections.ContainsKey(userId))
            {
                _userConnections[userId] = new HashSet<string>();
            }
            _userConnections[userId].Add(connectionId);
            _connectionUsers[connectionId] = userId;
        }

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (_userConnections.ContainsKey(userId))
            {
                _userConnections[userId].Remove(connectionId);
                if (_userConnections[userId].Count == 0)
                {
                    _userConnections.Remove(userId);
                }
            }
            _connectionUsers.Remove(connectionId);
        }

        return Task.CompletedTask;
    }

    public Task<List<string>> GetConnectionsAsync(Guid userId)
    {
        lock (_lock)
        {
            return Task.FromResult(_userConnections.ContainsKey(userId)
                ? _userConnections[userId].ToList()
                : new List<string>());
        }
    }

    public Task<Guid?> GetUserIdByConnectionAsync(string connectionId)
    {
        lock (_lock)
        {
            return Task.FromResult(_connectionUsers.ContainsKey(connectionId)
                ? _connectionUsers[connectionId]
                : null);
        }
    }

    public Task<List<Guid>> GetConnectedUsersAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_userConnections.Keys.ToList());
        }
    }
}
