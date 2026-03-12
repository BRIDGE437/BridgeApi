using BridgeApi.Application.Abstractions.Repositories.Post;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Post.GetPostById;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQueryRequest, GetPostByIdQueryResponse?>
{
    private readonly IPostReadRepository _postReadRepository;

    public GetPostByIdQueryHandler(IPostReadRepository postReadRepository)
    {
        _postReadRepository = postReadRepository;
    }

    public async Task<GetPostByIdQueryResponse?> Handle(GetPostByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var post = await _postReadRepository
            .GetAll(tracking: false)
            .Include(p => p.User)
            .Include(p => p.PostComments)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post == null)
            return null;

        return new GetPostByIdQueryResponse(
            post.Id,
            post.UserId,
            post.Content,
            post.ImageUrl,
            post.LikeCount,
            post.CommentCount,
            post.CreatedAt);
    }
}
