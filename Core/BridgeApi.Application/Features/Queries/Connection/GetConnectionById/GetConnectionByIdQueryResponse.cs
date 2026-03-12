namespace BridgeApi.Application.Features.Queries.Connection.GetConnectionById;

public record GetConnectionByIdQueryResponse(
    Guid Id,
    string SenderId,
    string ReceiverId,
    Guid IntentId,
    string? Note,
    int Status,
    DateTime CreatedAt);
