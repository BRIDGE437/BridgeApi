using BridgeApi.Application.Abstractions.Caching;
using MediatR;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;

public record GetUserProfileByUserIdQueryRequest(string UserId) : IRequest<GetUserProfileByUserIdQueryResponse?>, ICacheableQuery
{
    public string CacheKey => $"userprofiles:user:{UserId}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
