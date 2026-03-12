using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Connection.GetConnectionById;

public record GetConnectionByIdQueryRequest(Guid Id) : IRequest<GetConnectionByIdQueryResponse?>, ICacheableQuery
{
    public string CacheKey => $"connections:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(2);
}
