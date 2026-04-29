using BridgeApi.Application.Abstractions.Repositories.Notification;
using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Application.Abstractions.Repositories.PostComment;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationEntity = BridgeApi.Domain.Entities.Notification;
using PostCommentEntity = BridgeApi.Domain.Entities.PostComment;

namespace BridgeApi.Application.Features.Commands.PostComment.CreatePostComment;

public class CreatePostCommentCommandHandler : IRequestHandler<CreatePostCommentCommandRequest, CreatePostCommentCommandResponse>
{
    private readonly IPostCommentWriteRepository _postCommentWriteRepository;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ILogger<CreatePostCommentCommandHandler> _logger;

    public CreatePostCommentCommandHandler(
        IPostCommentWriteRepository postCommentWriteRepository,
        IPostReadRepository postReadRepository,
        IPostWriteRepository postWriteRepository,
        INotificationWriteRepository notificationWriteRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ILogger<CreatePostCommentCommandHandler> logger)
    {
        _postCommentWriteRepository = postCommentWriteRepository;
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
        _notificationWriteRepository = notificationWriteRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }

    public async Task<CreatePostCommentCommandResponse> Handle(CreatePostCommentCommandRequest request, CancellationToken cancellationToken)
    {
        var comment = new PostCommentEntity
        {
            PostId = request.PostId,
            UserId = request.UserId,
            CommentText = request.CommentText
        };

        await _postCommentWriteRepository.AddAsync(comment);
        await _postCommentWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {UserId} commented on Post {PostId}",
            request.UserId,
            request.PostId);

        var post = await _postReadRepository.GetByIdAsync(request.PostId, tracking: true);
        if (post != null)
        {
            post.CommentCount++;
            await _postWriteRepository.UpdateAsync(post);
            await _postWriteRepository.SaveAsync();

            // Notification + push (skip if own post)
            if (post.UserId != request.UserId)
            {
                var notification = new NotificationEntity
                {
                    UserId = post.UserId,
                    ActorId = request.UserId,
                    Type = NotificationType.PostCommented,
                    ReferenceId = post.Id,
                    Message = request.CommentText.Length > 100
                        ? request.CommentText[..100] + "..."
                        : request.CommentText
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

        return new CreatePostCommentCommandResponse(
            comment.Id,
            comment.PostId,
            comment.UserId,
            comment.CommentText,
            comment.CreatedAt);
    }
}
