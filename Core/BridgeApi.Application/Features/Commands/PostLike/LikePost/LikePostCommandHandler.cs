using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Application.Abstractions.Repositories.PostLike;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;
using PostLikeEntity = BridgeApi.Domain.Entities.PostLike;

namespace BridgeApi.Application.Features.Commands.PostLike.LikePost;

public class LikePostCommandHandler : IRequestHandler<LikePostCommandRequest, LikePostCommandResponse>
{
    private readonly IPostLikeReadRepository _postLikeReadRepository;
    private readonly IPostLikeWriteRepository _postLikeWriteRepository;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ILogger<LikePostCommandHandler> _logger;

    public LikePostCommandHandler(
        IPostLikeReadRepository postLikeReadRepository,
        IPostLikeWriteRepository postLikeWriteRepository,
        IPostReadRepository postReadRepository,
        IPostWriteRepository postWriteRepository,
        INotificationWriteRepository notificationWriteRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ILogger<LikePostCommandHandler> logger)
    {
        _postLikeReadRepository = postLikeReadRepository;
        _postLikeWriteRepository = postLikeWriteRepository;
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
        _notificationWriteRepository = notificationWriteRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public async Task<LikePostCommandResponse> Handle(LikePostCommandRequest request, CancellationToken cancellationToken)
    {
        var exists = await _postLikeReadRepository
            .GetWhere(pl => pl.PostId == request.PostId && pl.UserId == request.UserId, tracking: false)
            .AnyAsync(cancellationToken);
        if (exists)
            return new LikePostCommandResponse();

        var postLike = new PostLikeEntity
        {
            PostId = request.PostId,
            UserId = request.UserId
        };

        await _postLikeWriteRepository.AddAsync(postLike);
        await _postLikeWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {UserId} liked Post {PostId}",
            request.UserId,
            request.PostId);

        var post = await _postReadRepository.GetByIdAsync(request.PostId, tracking: true);
        if (post != null)
        {
            post.LikeCount++;
            await _postWriteRepository.UpdateAsync(post);
            await _postWriteRepository.SaveAsync();

            // Notification + push (skip if own post)
            if (post.UserId != request.UserId)
            {
                var notification = new NotificationEntity
                {
                    UserId = post.UserId,
                    ActorId = request.UserId,
                    Type = NotificationType.PostLiked,
                    ReferenceId = post.Id
                };

                await _notificationWriteRepository.AddAsync(notification);
                await _notificationWriteRepository.SaveAsync();

                await _realtimeNotificationService.SendNotificationAsync(post.UserId, new
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
        }

        return new LikePostCommandResponse();
    }
}
