using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Queries.Notification.GetNotifications;

public record GetNotificationsQueryResponse(PaginatedResponse<NotificationDto> Data);

public record NotificationDto(
    Guid Id,
    string UserId,
    string ActorId,
    string? ActorUserName,
    string? ActorProfileImage,
    NotificationType Type,
    Guid? ReferenceId,
    string? Message,
    bool IsRead,
    DateTime CreatedAt);
