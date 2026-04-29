using MediatR;

namespace BridgeApi.Application.Features.Queries.Notification.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQueryRequest(string UserId) : IRequest<GetUnreadNotificationCountQueryResponse>;
