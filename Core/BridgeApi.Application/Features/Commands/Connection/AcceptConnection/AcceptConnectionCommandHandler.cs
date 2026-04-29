using BridgeApi.Application.Abstractions.Repositories.Connection;
using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Application.Features.Commands.Connection.AcceptConnection;

public class AcceptConnectionCommandHandler : IRequestHandler<AcceptConnectionCommandRequest, AcceptConnectionCommandResponse?>
{
    private readonly IConnectionReadRepository _connectionReadRepository;
    private readonly IConnectionWriteRepository _connectionWriteRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ILogger<AcceptConnectionCommandHandler> _logger;

    public AcceptConnectionCommandHandler(
        IConnectionReadRepository connectionReadRepository,
        IConnectionWriteRepository connectionWriteRepository,
        INotificationWriteRepository notificationWriteRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ILogger<AcceptConnectionCommandHandler> logger)
    {
        _connectionReadRepository = connectionReadRepository;
        _connectionWriteRepository = connectionWriteRepository;
        _notificationWriteRepository = notificationWriteRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public async Task<AcceptConnectionCommandResponse?> Handle(AcceptConnectionCommandRequest request, CancellationToken cancellationToken)
    {
        var connection = await _connectionReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (connection == null)
            return null;

        if (connection.ReceiverId != request.RequestingUserId)
            throw new UnauthorizedAccessException("You do not have permission to accept this connection request.");

        connection.Status = 1; // Accepted

        await _connectionWriteRepository.UpdateAsync(connection);
        await _connectionWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Connection {ConnectionId} accepted (Sender: {SenderId}, Receiver: {ReceiverId})",
            connection.Id,
            connection.SenderId,
            connection.ReceiverId);

        // Notification + real-time push to the original sender
        var notification = new NotificationEntity
        {
            UserId = connection.SenderId,
            ActorId = connection.ReceiverId,
            Type = NotificationType.ConnectionAccepted,
            ReferenceId = connection.Id
        };

        await _notificationWriteRepository.AddAsync(notification);
        await _notificationWriteRepository.SaveAsync();

        await _realtimeNotificationService.SendNotificationAsync(connection.SenderId, new
        {
            notification.Id,
            notification.UserId,
            notification.ActorId,
            notification.Type,
            notification.ReferenceId,
            notification.Message,
            notification.IsRead,
            notification.CreatedAt
        });

        return new AcceptConnectionCommandResponse(
            connection.Id,
            connection.SenderId,
            connection.ReceiverId,
            connection.IntentId,
            connection.Note,
            connection.Status,
            connection.CreatedAt);
    }
}
