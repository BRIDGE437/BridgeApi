using BridgeApi.Application.Abstractions.Caching;
using BridgeApi.Application.Abstractions.Pagination;
using MediatR;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetAllUserProfiles;

public record GetAllUserProfilesQueryRequest(PaginationRequest Pagination) : IRequest<GetAllUserProfilesQueryResponse>, ICacheableQuery
{
    public GetAllUserProfilesQueryRequest() : this(new PaginationRequest()) { }
    public string CacheKey => $"userprofiles:all:p{Pagination.Page}:s{Pagination.Size}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
