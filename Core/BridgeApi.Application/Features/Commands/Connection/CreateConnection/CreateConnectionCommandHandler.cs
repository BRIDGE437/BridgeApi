using BridgeApi.Application.Abstractions.Repositories.Connection;
using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using ConnectionEntity = BridgeApi.Domain.Entities.Connection;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Application.Features.Commands.Connection.CreateConnection;

public class CreateConnectionCommandHandler : IRequestHandler<CreateConnectionCommandRequest, CreateConnectionCommandResponse>
{
    private readonly IConnectionWriteRepository _connectionWriteRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ILogger<CreateConnectionCommandHandler> _logger;

    public CreateConnectionCommandHandler(
        IConnectionWriteRepository connectionWriteRepository,
        INotificationWriteRepository notificationWriteRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ILogger<CreateConnectionCommandHandler> logger)
    {
        _connectionWriteRepository = connectionWriteRepository;
        _notificationWriteRepository = notificationWriteRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public async Task<CreateConnectionCommandResponse> Handle(CreateConnectionCommandRequest request, CancellationToken cancellationToken)
    {
        var connection = new ConnectionEntity
        {
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            IntentId = request.IntentId,
            Note = request.Note,
            Status = 0 // Pending
        };

        await _connectionWriteRepository.AddAsync(connection);
        await _connectionWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Connection request created: {SenderId} -> {ReceiverId} (Intent: {IntentId})",
            request.SenderId,
            request.ReceiverId,
            request.IntentId);

        // Notification + real-time push to receiver
        var notification = new NotificationEntity
        {
            UserId = request.ReceiverId,
            ActorId = request.SenderId,
            Type = NotificationType.ConnectionRequest,
            ReferenceId = connection.Id
        };

        await _notificationWriteRepository.AddAsync(notification);
        await _notificationWriteRepository.SaveAsync();

        await _realtimeNotificationService.SendNotificationAsync(request.ReceiverId, new
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

        return new CreateConnectionCommandResponse(
            connection.Id,
            connection.SenderId,
            connection.ReceiverId,
            connection.IntentId,
            connection.Note,
            connection.Status,
            connection.CreatedAt);
    }
}
