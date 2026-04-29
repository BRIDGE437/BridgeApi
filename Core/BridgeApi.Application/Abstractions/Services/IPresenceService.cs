namespace BridgeApi.Application.Abstractions.Services;

public interface IPresenceService
{
    Task UserConnectedAsync(string userId, string connectionId);
    Task UserDisconnectedAsync(string userId, string connectionId);
    Task<bool> IsUserOnlineAsync(string userId);
    Task<Dictionary<string, bool>> GetOnlineStatusesAsync(IEnumerable<string> userIds);
}
