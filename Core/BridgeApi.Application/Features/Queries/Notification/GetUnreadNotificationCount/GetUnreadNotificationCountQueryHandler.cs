using BridgeApi.Application.Abstractions.Repositories.Notification;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Notification.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQueryRequest, GetUnreadNotificationCountQueryResponse>
{
    private readonly INotificationReadRepository _notificationReadRepository;

    public GetUnreadNotificationCountQueryHandler(INotificationReadRepository notificationReadRepository)
    {
        _notificationReadRepository = notificationReadRepository;
    }

    public async Task<GetUnreadNotificationCountQueryResponse> Handle(GetUnreadNotificationCountQueryRequest request, CancellationToken cancellationToken)
    {
        var count = await _notificationReadRepository
            .GetWhere(n => n.UserId == request.UserId && !n.IsRead, tracking: false)
            .CountAsync(cancellationToken);

        return new GetUnreadNotificationCountQueryResponse(count);
    }
}
