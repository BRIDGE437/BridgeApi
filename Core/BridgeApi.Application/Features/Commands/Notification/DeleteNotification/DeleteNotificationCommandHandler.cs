using BridgeApi.Application.Abstractions.Repositories.Notification;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Notification.DeleteNotification;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommandRequest, DeleteNotificationCommandResponse?>
{
    private readonly INotificationWriteRepository _notificationWriteRepository;

    public DeleteNotificationCommandHandler(INotificationWriteRepository notificationWriteRepository)
    {
        _notificationWriteRepository = notificationWriteRepository;
    }

    public async Task<DeleteNotificationCommandResponse?> Handle(DeleteNotificationCommandRequest request, CancellationToken cancellationToken)
    {
        var removed = await _notificationWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _notificationWriteRepository.SaveAsync();
        return new DeleteNotificationCommandResponse();
    }
}
