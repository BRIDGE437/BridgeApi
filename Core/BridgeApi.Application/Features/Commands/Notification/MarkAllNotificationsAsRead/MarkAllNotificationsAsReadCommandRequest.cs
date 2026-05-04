using MediatR;

namespace BridgeApi.Application.Features.Commands.Notification.MarkAllNotificationsAsRead;

public record MarkAllNotificationsAsReadCommandRequest(string UserId) : IRequest<MarkAllNotificationsAsReadCommandResponse>;
