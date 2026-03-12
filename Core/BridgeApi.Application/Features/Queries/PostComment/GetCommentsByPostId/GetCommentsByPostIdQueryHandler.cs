using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Abstractions.Repositories.PostComment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.PostComment.GetCommentsByPostId;

public class GetCommentsByPostIdQueryHandler : IRequestHandler<GetCommentsByPostIdQueryRequest, GetCommentsByPostIdQueryResponse>
{
    private readonly IPostCommentReadRepository _postCommentReadRepository;

    public GetCommentsByPostIdQueryHandler(IPostCommentReadRepository postCommentReadRepository)
    {
        _postCommentReadRepository = postCommentReadRepository;
    }

    public async Task<GetCommentsByPostIdQueryResponse> Handle(GetCommentsByPostIdQueryRequest request, CancellationToken cancellationToken)
    {
        var paginatedResult = await _postCommentReadRepository
            .GetWhere(c => c.PostId == request.PostId, tracking: false)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new PostCommentDto(c.Id, c.PostId, c.UserId, c.CommentText, c.CreatedAt))
            .ToPaginatedListAsync(
                request.Pagination.Page,
                request.Pagination.Size,
                cancellationToken);

        return new GetCommentsByPostIdQueryResponse(paginatedResult);
    }
}
