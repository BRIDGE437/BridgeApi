using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.User.GetUserById;

public record GetUserByIdQueryRequest(string Id) : IRequest<GetUserByIdQueryResponse?>, ICacheableQuery
{
    public string CacheKey => $"users:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
