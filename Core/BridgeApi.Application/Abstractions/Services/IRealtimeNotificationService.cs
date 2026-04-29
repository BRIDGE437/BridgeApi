namespace BridgeApi.Application.Abstractions.Services;

public interface IRealtimeNotificationService
{
    Task SendNotificationAsync(string userId, object notification);
    Task SendMessageAsync(string userId, object message);
    Task SendTypingIndicatorAsync(Guid connectionId, string userId, bool isTyping);
    Task SendUserOnlineAsync(string userId);
    Task SendUserOfflineAsync(string userId);
}
