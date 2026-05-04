using BridgeApi.Application.Abstractions.Pagination;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Notification.GetNotifications;

public record GetNotificationsQueryRequest(string UserId, PaginationRequest Pagination) : IRequest<GetNotificationsQueryResponse>
{
    public GetNotificationsQueryRequest(string userId) : this(userId, new PaginationRequest()) { }
}
