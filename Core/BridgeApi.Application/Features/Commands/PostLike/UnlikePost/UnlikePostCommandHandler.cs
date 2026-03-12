using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Application.Abstractions.Repositories.PostLike;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.PostLike.UnlikePost;

public class UnlikePostCommandHandler : IRequestHandler<UnlikePostCommandRequest, UnlikePostCommandResponse>
{
    private readonly IPostLikeReadRepository _postLikeReadRepository;
    private readonly IPostLikeWriteRepository _postLikeWriteRepository;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly ILogger<UnlikePostCommandHandler> _logger;

    public UnlikePostCommandHandler(
        IPostLikeReadRepository postLikeReadRepository,
        IPostLikeWriteRepository postLikeWriteRepository,
        IPostReadRepository postReadRepository,
        IPostWriteRepository postWriteRepository,
        ILogger<UnlikePostCommandHandler> logger)
    {
        _postLikeReadRepository = postLikeReadRepository;
        _postLikeWriteRepository = postLikeWriteRepository;
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
        _logger = logger;
    }

    public async Task<UnlikePostCommandResponse> Handle(UnlikePostCommandRequest request, CancellationToken cancellationToken)
    {
        var postLike = await _postLikeReadRepository
            .GetWhere(pl => pl.PostId == request.PostId && pl.UserId == request.UserId, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        if (postLike != null)
        {
            await _postLikeWriteRepository.RemoveAsync(postLike);
            await _postLikeWriteRepository.SaveAsync();

            _logger.LogInformation(
                "User {UserId} unliked Post {PostId}",
                request.UserId,
                request.PostId);
        }

        var post = await _postReadRepository.GetByIdAsync(request.PostId, tracking: true);
        if (post != null && post.LikeCount > 0)
        {
            post.LikeCount--;
            await _postWriteRepository.UpdateAsync(post);
            await _postWriteRepository.SaveAsync();
        }

        return new UnlikePostCommandResponse();
    }
}
