using BridgeApi.Application.Abstractions.Pagination;

namespace BridgeApi.Application.Features.Queries.Message.GetMessagesByConnectionId;

public record GetMessagesByConnectionIdQueryResponse(PaginatedResponse<MessageDto> Data);

public record MessageDto(
    Guid Id,
    Guid ConnectionId,
    string SenderId,
    string Content,
    bool IsRead,
    DateTime CreatedAt);
