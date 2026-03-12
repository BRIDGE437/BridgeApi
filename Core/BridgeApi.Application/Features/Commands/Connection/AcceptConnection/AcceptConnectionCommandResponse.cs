namespace BridgeApi.Application.Features.Commands.Connection.AcceptConnection;

public record AcceptConnectionCommandResponse(
    Guid Id,
    string SenderId,
    string ReceiverId,
    Guid IntentId,
    string? Note,
    int Status,
    DateTime CreatedAt);
