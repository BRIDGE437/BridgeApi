using BridgeApi.Application.Abstractions.Pagination;

namespace BridgeApi.Application.Features.Queries.Connection.GetAllConnections;

public record GetAllConnectionsQueryResponse(PaginatedResponse<ConnectionDto> Data);

public record ConnectionDto(
    Guid Id,
    string SenderId,
    string ReceiverId,
    Guid IntentId,
    string? Note,
    int Status,
    DateTime CreatedAt);
