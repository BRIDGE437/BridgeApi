namespace BridgeApi.Application.Features.Commands.Connection.RejectConnection;

public record RejectConnectionCommandResponse(
    Guid Id,
    string SenderId,
    string ReceiverId,
    Guid IntentId,
    string? Note,
    int Status,
    DateTime CreatedAt);
