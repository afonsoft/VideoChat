namespace FamilyMeet.HttpApi.Services;

public interface IConnectionManager
{
    Task AddConnectionAsync(Guid userId, string connectionId);
    Task RemoveConnectionAsync(Guid userId, string connectionId);
    Task<List<string>> GetConnectionsAsync(Guid userId);
}

public class ConnectionManager : IConnectionManager
{
    private readonly Dictionary<Guid, HashSet<string>> _userConnections = new();
    private readonly object _lock = new object();

    public Task AddConnectionAsync(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_userConnections.ContainsKey(userId))
            {
                _userConnections[userId] = new HashSet<string>();
            }
            _userConnections[userId].Add(connectionId);
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
}
