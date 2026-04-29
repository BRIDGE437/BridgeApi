using MediatR;

namespace BridgeApi.Application.Features.Commands.Notification.MarkNotificationAsRead;

public record MarkNotificationAsReadCommandRequest(Guid Id) : IRequest<MarkNotificationAsReadCommandResponse?>;
