using MediatR;

namespace BridgeApi.Application.Features.Commands.Notification.DeleteNotification;

public record DeleteNotificationCommandRequest(Guid Id) : IRequest<DeleteNotificationCommandResponse?>;
