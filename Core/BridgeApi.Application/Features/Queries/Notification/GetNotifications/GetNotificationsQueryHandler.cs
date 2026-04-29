using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Abstractions.Repositories.Notification;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Notification.GetNotifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQueryRequest, GetNotificationsQueryResponse>
{
    private readonly INotificationReadRepository _notificationReadRepository;

    public GetNotificationsQueryHandler(INotificationReadRepository notificationReadRepository)
    {
        _notificationReadRepository = notificationReadRepository;
    }

    public async Task<GetNotificationsQueryResponse> Handle(GetNotificationsQueryRequest request, CancellationToken cancellationToken)
    {
        var paginatedResult = await _notificationReadRepository
            .GetWhere(n => n.UserId == request.UserId, tracking: false)
            .Include(n => n.Actor)
                .ThenInclude(a => a.UserProfile)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.Id,
                n.UserId,
                n.ActorId,
                n.Actor.UserName,
                n.Actor.UserProfile != null ? n.Actor.UserProfile.ProfileImage : null,
                n.Type,
                n.ReferenceId,
                n.Message,
                n.IsRead,
                n.CreatedAt))
            .ToPaginatedListAsync(
                request.Pagination.Page,
                request.Pagination.Size,
                cancellationToken);

        return new GetNotificationsQueryResponse(paginatedResult);
    }
}
