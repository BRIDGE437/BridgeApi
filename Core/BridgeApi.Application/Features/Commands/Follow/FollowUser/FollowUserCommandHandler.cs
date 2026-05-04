using BridgeApi.Application.Abstractions.Repositories.Follow;
using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using FollowEntity = BridgeApi.Domain.Entities.Follow;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;

namespace BridgeApi.Application.Features.Commands.Follow.FollowUser;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommandRequest, FollowUserCommandResponse>
{
    private readonly IFollowWriteRepository _followWriteRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ILogger<FollowUserCommandHandler> _logger;

    public FollowUserCommandHandler(
        IFollowWriteRepository followWriteRepository,
        INotificationWriteRepository notificationWriteRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ILogger<FollowUserCommandHandler> logger)
    {
        _followWriteRepository = followWriteRepository;
        _notificationWriteRepository = notificationWriteRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public async Task<FollowUserCommandResponse> Handle(FollowUserCommandRequest request, CancellationToken cancellationToken)
    {
        var follow = new FollowEntity
        {
            FollowerId = request.FollowerId,
            FollowingId = request.FollowingId
        };

        await _followWriteRepository.AddAsync(follow);
        await _followWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {FollowerId} followed User {FollowingId}",
            request.FollowerId,
            request.FollowingId);

        // Notification + push to followed user
        var notification = new NotificationEntity
        {
            UserId = request.FollowingId,
            ActorId = request.FollowerId,
            Type = NotificationType.NewFollower
        };

        await _notificationWriteRepository.AddAsync(notification);
        await _notificationWriteRepository.SaveAsync();

        await _realtimeNotificationService.SendNotificationAsync(request.FollowingId, new
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

        return new FollowUserCommandResponse();
    }
}
