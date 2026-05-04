using BridgeApi.Application.Abstractions.Repositories.Notification;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Application.Features.Commands.Notification.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommandRequest, CreateNotificationCommandResponse>
{
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly ILogger<CreateNotificationCommandHandler> _logger;

    public CreateNotificationCommandHandler(
        INotificationWriteRepository notificationWriteRepository,
        ILogger<CreateNotificationCommandHandler> logger)
    {
        _notificationWriteRepository = notificationWriteRepository;
        _logger = logger;
    }

    public async Task<CreateNotificationCommandResponse> Handle(CreateNotificationCommandRequest request, CancellationToken cancellationToken)
    {
        var notification = new NotificationEntity
        {
            UserId = request.UserId,
            ActorId = request.ActorId,
            Type = request.Type,
            ReferenceId = request.ReferenceId,
            Message = request.Message,
            IsRead = false
        };

        await _notificationWriteRepository.AddAsync(notification);
        await _notificationWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Notification created for User {UserId} by Actor {ActorId}, Type: {NotificationType}",
            request.UserId,
            request.ActorId,
            request.Type);

        return new CreateNotificationCommandResponse(
            notification.Id,
            notification.UserId,
            notification.ActorId,
            notification.Type,
            notification.ReferenceId,
            notification.Message,
            notification.IsRead,
            notification.CreatedAt);
    }
}
