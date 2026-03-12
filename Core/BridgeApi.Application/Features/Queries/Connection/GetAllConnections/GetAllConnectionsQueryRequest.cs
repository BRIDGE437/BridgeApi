using BridgeApi.Application.Abstractions.Caching;
using BridgeApi.Application.Abstractions.Pagination;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Connection.GetAllConnections;

public record GetAllConnectionsQueryRequest(
    string? SenderId,
    string? ReceiverId,
    int? Status,
    PaginationRequest Pagination) : IRequest<GetAllConnectionsQueryResponse>, ICacheableQuery
{
    public GetAllConnectionsQueryRequest(string? senderId, string? receiverId, int? status)
        : this(senderId, receiverId, status, new PaginationRequest()) { }

    public string CacheKey => $"connections:all:{SenderId}:{ReceiverId}:{Status}:p{Pagination.Page}:s{Pagination.Size}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(2);
}
