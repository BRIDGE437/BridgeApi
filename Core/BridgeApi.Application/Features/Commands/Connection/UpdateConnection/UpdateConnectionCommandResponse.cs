namespace BridgeApi.Application.Features.Commands.Connection.UpdateConnection;

public record UpdateConnectionCommandResponse(
    Guid Id,
    string SenderId,
    string ReceiverId,
    Guid IntentId,
    string? Note,
    int Status,
    DateTime CreatedAt);
