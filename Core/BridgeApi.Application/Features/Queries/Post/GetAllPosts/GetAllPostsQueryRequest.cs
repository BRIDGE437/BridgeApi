using BridgeApi.Application.Abstractions.Caching;
using BridgeApi.Application.Abstractions.Pagination;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Post.GetAllPosts;

public record GetAllPostsQueryRequest(PaginationRequest Pagination) : IRequest<GetAllPostsQueryResponse>, ICacheableQuery
{
    public GetAllPostsQueryRequest() : this(new PaginationRequest()) { }
    public string CacheKey => $"posts:all:p{Pagination.Page}:s{Pagination.Size}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(2);
}
