using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.UserIntent.GetUserIntents;

public record GetUserIntentsQueryRequest(string UserId) : IRequest<GetUserIntentsQueryResponse>, ICacheableQuery
{
    public string CacheKey => $"userintents:user:{UserId}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
