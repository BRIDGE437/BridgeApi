using BridgeApi.Application.Abstractions.Caching;
using BridgeApi.Application.Abstractions.Pagination;
using MediatR;

namespace BridgeApi.Application.Features.Queries.PostComment.GetCommentsByPostId;

public record GetCommentsByPostIdQueryRequest(Guid PostId, PaginationRequest Pagination) : IRequest<GetCommentsByPostIdQueryResponse>, ICacheableQuery
{
    public GetCommentsByPostIdQueryRequest(Guid postId) : this(postId, new PaginationRequest()) { }
    public string CacheKey => $"comments:post:{PostId}:p{Pagination.Page}:s{Pagination.Size}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(2);
}
