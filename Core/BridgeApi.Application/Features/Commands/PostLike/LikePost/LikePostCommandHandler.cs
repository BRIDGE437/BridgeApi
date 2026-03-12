using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Application.Abstractions.Repositories.PostLike;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PostLikeEntity = BridgeApi.Domain.Entities.PostLike;

namespace BridgeApi.Application.Features.Commands.PostLike.LikePost;

public class LikePostCommandHandler : IRequestHandler<LikePostCommandRequest, LikePostCommandResponse>
{
    private readonly IPostLikeReadRepository _postLikeReadRepository;
    private readonly IPostLikeWriteRepository _postLikeWriteRepository;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly ILogger<LikePostCommandHandler> _logger;

    public LikePostCommandHandler(
        IPostLikeReadRepository postLikeReadRepository,
        IPostLikeWriteRepository postLikeWriteRepository,
        IPostReadRepository postReadRepository,
        IPostWriteRepository postWriteRepository,
        ILogger<LikePostCommandHandler> logger)
    {
        _postLikeReadRepository = postLikeReadRepository;
        _postLikeWriteRepository = postLikeWriteRepository;
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
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
        }

        return new LikePostCommandResponse();
    }
}
