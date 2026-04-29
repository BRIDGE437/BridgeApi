namespace BridgeApi.Application.Features.Commands.Notification.MarkNotificationAsRead;

public record MarkNotificationAsReadCommandResponse(Guid Id, bool IsRead);
