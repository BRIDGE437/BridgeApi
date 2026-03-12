using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Intent.GetIntentById;

public record GetIntentByIdQueryRequest(Guid Id) : IRequest<GetIntentByIdQueryResponse?>, ICacheableQuery
{
    public string CacheKey => $"intents:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(30);
}
