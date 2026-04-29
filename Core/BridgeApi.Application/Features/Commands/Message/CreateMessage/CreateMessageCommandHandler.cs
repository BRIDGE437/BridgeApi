using BridgeApi.Application.Abstractions.Repositories.Connection;
using BridgeApi.Application.Abstractions.Repositories.Message;
using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using MessageEntity = BridgeApi.Domain.Entities.Message;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Application.Features.Commands.Message.CreateMessage;

public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommandRequest, CreateMessageCommandResponse>
{
    private readonly IMessageWriteRepository _messageWriteRepository;
    private readonly IConnectionReadRepository _connectionReadRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ILogger<CreateMessageCommandHandler> _logger;

    public CreateMessageCommandHandler(
        IMessageWriteRepository messageWriteRepository,
        IConnectionReadRepository connectionReadRepository,
        INotificationWriteRepository notificationWriteRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ILogger<CreateMessageCommandHandler> logger)
    {
        _messageWriteRepository = messageWriteRepository;
        _connectionReadRepository = connectionReadRepository;
        _notificationWriteRepository = notificationWriteRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public async Task<CreateMessageCommandResponse> Handle(CreateMessageCommandRequest request, CancellationToken cancellationToken)
    {
        var message = new MessageEntity
        {
            ConnectionId = request.ConnectionId,
            SenderId = request.SenderId,
            Content = request.Content,
            IsRead = false
        };

        await _messageWriteRepository.AddAsync(message);
        await _messageWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Message sent in Connection {ConnectionId} by User {SenderId}",
            request.ConnectionId,
            request.SenderId);

        // Real-time push to recipient
        var connection = await _connectionReadRepository.GetByIdAsync(request.ConnectionId, tracking: false);
        if (connection != null)
        {
            var recipientId = connection.SenderId == request.SenderId
                ? connection.ReceiverId
                : connection.SenderId;

            var messagePayload = new
            {
                message.Id,
                message.ConnectionId,
                message.SenderId,
                message.Content,
                message.IsRead,
                message.CreatedAt
            };

            await _realtimeNotificationService.SendMessageAsync(recipientId, messagePayload);

            // Create notification
            var notification = new NotificationEntity
            {
                UserId = recipientId,
                ActorId = request.SenderId,
                Type = NotificationType.NewMessage,
                ReferenceId = message.ConnectionId,
                Message = request.Content.Length > 100
                    ? request.Content[..100] + "..."
                    : request.Content
            };

            await _notificationWriteRepository.AddAsync(notification);
            await _notificationWriteRepository.SaveAsync();

            await _realtimeNotificationService.SendNotificationAsync(recipientId, new
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
        }

        return new CreateMessageCommandResponse(
            message.Id,
            message.ConnectionId,
            message.SenderId,
            message.Content,
            message.IsRead,
            message.CreatedAt);
    }
}
