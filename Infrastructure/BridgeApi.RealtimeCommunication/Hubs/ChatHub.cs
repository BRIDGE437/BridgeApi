using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.RealtimeCommunication.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            _logger.LogInformation("User {UserId} connected to ChatHub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            _logger.LogInformation("User {UserId} disconnected from ChatHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConnection(Guid connectionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"connection:{connectionId}");
        _logger.LogInformation("User {UserId} joined connection group {ConnectionId}", Context.UserIdentifier, connectionId);
    }

    public async Task LeaveConnection(Guid connectionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"connection:{connectionId}");
        _logger.LogInformation("User {UserId} left connection group {ConnectionId}", Context.UserIdentifier, connectionId);
    }

    public async Task UserTyping(Guid connectionId, bool isTyping)
    {
        await Clients.OthersInGroup($"connection:{connectionId}")
            .SendAsync("UserTyping", Context.UserIdentifier, isTyping);
    }
}
