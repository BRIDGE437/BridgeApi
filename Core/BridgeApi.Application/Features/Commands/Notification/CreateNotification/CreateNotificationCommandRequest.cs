using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Notification.CreateNotification;

public record CreateNotificationCommandRequest(
    string UserId,
    string ActorId,
    NotificationType Type,
    Guid? ReferenceId,
    string? Message) : IRequest<CreateNotificationCommandResponse>;
