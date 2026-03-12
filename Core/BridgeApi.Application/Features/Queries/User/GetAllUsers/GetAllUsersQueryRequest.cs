using BridgeApi.Application.Abstractions.Caching;
using BridgeApi.Application.Abstractions.Pagination;
using MediatR;

namespace BridgeApi.Application.Features.Queries.User.GetAllUsers;

public record GetAllUsersQueryRequest(PaginationRequest Pagination) : IRequest<GetAllUsersQueryResponse>, ICacheableQuery
{
    public GetAllUsersQueryRequest() : this(new PaginationRequest()) { }
    public string CacheKey => $"users:all:p{Pagination.Page}:s{Pagination.Size}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
