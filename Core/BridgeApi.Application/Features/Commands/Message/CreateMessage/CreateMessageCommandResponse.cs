namespace BridgeApi.Application.Features.Commands.Message.CreateMessage;

public record CreateMessageCommandResponse(
    Guid Id,
    Guid ConnectionId,
    string SenderId,
    string Content,
    bool IsRead,
    DateTime CreatedAt);
