using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.Notification.CreateNotification;

public record CreateNotificationCommandResponse(
    Guid Id,
    string UserId,
    string ActorId,
    NotificationType Type,
    Guid? ReferenceId,
    string? Message,
    bool IsRead,
    DateTime CreatedAt);
