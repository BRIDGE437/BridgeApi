using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Post.GetPostById;

public record GetPostByIdQueryRequest(Guid Id) : IRequest<GetPostByIdQueryResponse?>, ICacheableQuery
{
    public string CacheKey => $"posts:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(2);
}
