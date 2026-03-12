using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Intent.GetAllIntents;

public record GetAllIntentsQueryRequest : IRequest<GetAllIntentsQueryResponse>, ICacheableQuery
{
    public string CacheKey => "intents:all";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(30);
}
