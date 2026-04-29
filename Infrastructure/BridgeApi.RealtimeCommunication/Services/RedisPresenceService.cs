using BridgeApi.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BridgeApi.RealtimeCommunication.Services;

public class RedisPresenceService : IPresenceService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisPresenceService> _logger;

    public RedisPresenceService(IConnectionMultiplexer redis, ILogger<RedisPresenceService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private static string GetKey(string userId) => $"presence:user:{userId}";

    public async Task UserConnectedAsync(string userId, string connectionId)
    {
        var db = _redis.GetDatabase();
        await db.SetAddAsync(GetKey(userId), connectionId);
        _logger.LogInformation("Presence: User {UserId} added connection {ConnectionId}", userId, connectionId);
    }

    public async Task UserDisconnectedAsync(string userId, string connectionId)
    {
        var db = _redis.GetDatabase();
        await db.SetRemoveAsync(GetKey(userId), connectionId);
        _logger.LogInformation("Presence: User {UserId} removed connection {ConnectionId}", userId, connectionId);
    }

    public async Task<bool> IsUserOnlineAsync(string userId)
    {
        var db = _redis.GetDatabase();
        var count = await db.SetLengthAsync(GetKey(userId));
        return count > 0;
    }

    public async Task<Dictionary<string, bool>> GetOnlineStatusesAsync(IEnumerable<string> userIds)
    {
        var db = _redis.GetDatabase();
        var result = new Dictionary<string, bool>();

        foreach (var userId in userIds)
        {
            var count = await db.SetLengthAsync(GetKey(userId));
            result[userId] = count > 0;
        }

        return result;
    }
}
