using BridgeApi.Application.Abstractions.Repositories.Notification;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Notification.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommandRequest, MarkAllNotificationsAsReadCommandResponse>
{
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly ILogger<MarkAllNotificationsAsReadCommandHandler> _logger;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationReadRepository notificationReadRepository,
        INotificationWriteRepository notificationWriteRepository,
        ILogger<MarkAllNotificationsAsReadCommandHandler> logger)
    {
        _notificationReadRepository = notificationReadRepository;
        _notificationWriteRepository = notificationWriteRepository;
        _logger = logger;
    }

    public async Task<MarkAllNotificationsAsReadCommandResponse> Handle(MarkAllNotificationsAsReadCommandRequest request, CancellationToken cancellationToken)
    {
        var unreadNotifications = await _notificationReadRepository
            .GetWhere(n => n.UserId == request.UserId && !n.IsRead, tracking: true)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _notificationWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Marked {Count} notifications as read for User {UserId}",
            unreadNotifications.Count,
            request.UserId);

        return new MarkAllNotificationsAsReadCommandResponse(unreadNotifications.Count);
    }
}
