namespace BridgeApi.Application.Features.Commands.Message.MarkMessageAsRead;

public record MarkMessageAsReadCommandResponse(
    Guid Id,
    Guid ConnectionId,
    string SenderId,
    string Content,
    bool IsRead,
    DateTime CreatedAt);
