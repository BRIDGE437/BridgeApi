using BridgeApi.Application.Abstractions.Pagination;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Message.GetMessagesByConnectionId;

public record GetMessagesByConnectionIdQueryRequest(Guid ConnectionId, PaginationRequest Pagination) : IRequest<GetMessagesByConnectionIdQueryResponse>
{
    public GetMessagesByConnectionIdQueryRequest(Guid connectionId) : this(connectionId, new PaginationRequest()) { }
}
