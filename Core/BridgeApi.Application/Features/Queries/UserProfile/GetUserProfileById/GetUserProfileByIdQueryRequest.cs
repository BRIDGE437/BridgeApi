using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileById;

public record GetUserProfileByIdQueryRequest(Guid Id) : IRequest<GetUserProfileByIdQueryResponse?>, ICacheableQuery
{
    public string CacheKey => $"userprofiles:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
