using BridgeApi.Application.Abstractions.Repositories.Notification;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Notification.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommandRequest, MarkNotificationAsReadCommandResponse?>
{
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly INotificationWriteRepository _notificationWriteRepository;

    public MarkNotificationAsReadCommandHandler(
        INotificationReadRepository notificationReadRepository,
        INotificationWriteRepository notificationWriteRepository)
    {
        _notificationReadRepository = notificationReadRepository;
        _notificationWriteRepository = notificationWriteRepository;
    }

    public async Task<MarkNotificationAsReadCommandResponse?> Handle(MarkNotificationAsReadCommandRequest request, CancellationToken cancellationToken)
    {
        var notification = await _notificationReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (notification == null)
            return null;

        notification.IsRead = true;

        await _notificationWriteRepository.UpdateAsync(notification);
        await _notificationWriteRepository.SaveAsync();

        return new MarkNotificationAsReadCommandResponse(notification.Id, notification.IsRead);
    }
}
