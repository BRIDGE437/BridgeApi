using BridgeApi.Application.Abstractions.Services;
using BridgeApi.RealtimeCommunication.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BridgeApi.RealtimeCommunication.Services;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly IHubContext<PresenceHub> _presenceHub;

    public RealtimeNotificationService(
        IHubContext<NotificationHub> notificationHub,
        IHubContext<ChatHub> chatHub,
        IHubContext<PresenceHub> presenceHub)
    {
        _notificationHub = notificationHub;
        _chatHub = chatHub;
        _presenceHub = presenceHub;
    }

    public async Task SendNotificationAsync(string userId, object notification)
    {
        await _notificationHub.Clients.Group($"user:{userId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendMessageAsync(string userId, object message)
    {
        await _chatHub.Clients.Group($"user:{userId}")
            .SendAsync("ReceiveMessage", message);
    }

    public async Task SendTypingIndicatorAsync(Guid connectionId, string userId, bool isTyping)
    {
        await _chatHub.Clients.Group($"connection:{connectionId}")
            .SendAsync("UserTyping", userId, isTyping);
    }

    public async Task SendUserOnlineAsync(string userId)
    {
        await _presenceHub.Clients.All
            .SendAsync("UserOnline", userId);
    }

    public async Task SendUserOfflineAsync(string userId)
    {
        await _presenceHub.Clients.All
            .SendAsync("UserOffline", userId);
    }
}
