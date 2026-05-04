using BridgeApi.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.RealtimeCommunication.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IPresenceService _presenceService;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ILogger<PresenceHub> _logger;

    public PresenceHub(
        IPresenceService presenceService,
        IRealtimeNotificationService realtimeNotificationService,
        ILogger<PresenceHub> logger)
    {
        _presenceService = presenceService;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await _presenceService.UserConnectedAsync(userId, Context.ConnectionId);
            await _realtimeNotificationService.SendUserOnlineAsync(userId);
            _logger.LogInformation("User {UserId} is now online", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await _presenceService.UserDisconnectedAsync(userId, Context.ConnectionId);

            var stillOnline = await _presenceService.IsUserOnlineAsync(userId);
            if (!stillOnline)
            {
                await _realtimeNotificationService.SendUserOfflineAsync(userId);
                _logger.LogInformation("User {UserId} is now offline", userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
