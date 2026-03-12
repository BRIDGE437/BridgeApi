using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Application.Abstractions.Repositories.PostComment;
using MediatR;
using Microsoft.Extensions.Logging;
using PostCommentEntity = BridgeApi.Domain.Entities.PostComment;

namespace BridgeApi.Application.Features.Commands.PostComment.CreatePostComment;

public class CreatePostCommentCommandHandler : IRequestHandler<CreatePostCommentCommandRequest, CreatePostCommentCommandResponse>
{
    private readonly IPostCommentWriteRepository _postCommentWriteRepository;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly ILogger<CreatePostCommentCommandHandler> _logger;

    public CreatePostCommentCommandHandler(
        IPostCommentWriteRepository postCommentWriteRepository,
        IPostReadRepository postReadRepository,
        IPostWriteRepository postWriteRepository,
        ILogger<CreatePostCommentCommandHandler> logger)
    {
        _postCommentWriteRepository = postCommentWriteRepository;
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
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
        }

        return new CreatePostCommentCommandResponse(
            comment.Id,
            comment.PostId,
            comment.UserId,
            comment.CommentText,
            comment.CreatedAt);
    }
}
