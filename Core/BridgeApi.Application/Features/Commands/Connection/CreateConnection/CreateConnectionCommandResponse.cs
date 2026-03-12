namespace BridgeApi.Application.Features.Commands.Connection.CreateConnection;

public record CreateConnectionCommandResponse(
    Guid Id,
    string SenderId,
    string ReceiverId,
    Guid IntentId,
    string? Note,
    int Status,
    DateTime CreatedAt);
