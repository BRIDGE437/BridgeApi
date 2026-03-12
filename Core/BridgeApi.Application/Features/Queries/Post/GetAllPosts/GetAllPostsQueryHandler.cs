using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Abstractions.Repositories.Post;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Post.GetAllPosts;

public class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQueryRequest, GetAllPostsQueryResponse>
{
    private readonly IPostReadRepository _postReadRepository;

    public GetAllPostsQueryHandler(IPostReadRepository postReadRepository)
    {
        _postReadRepository = postReadRepository;
    }

    public async Task<GetAllPostsQueryResponse> Handle(GetAllPostsQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _postReadRepository
            .GetAll(tracking: false)
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDto(
                p.Id,
                p.UserId,
                p.Content,
                p.ImageUrl,
                p.LikeCount,
                p.CommentCount,
                p.CreatedAt));

        var paginatedResult = await query.ToPaginatedListAsync(
            request.Pagination.Page,
            request.Pagination.Size,
            cancellationToken);

        return new GetAllPostsQueryResponse(paginatedResult);
    }
}
