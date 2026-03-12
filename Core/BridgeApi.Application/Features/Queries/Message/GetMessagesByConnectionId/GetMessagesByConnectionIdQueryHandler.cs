using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Abstractions.Repositories.Message;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Message.GetMessagesByConnectionId;

public class GetMessagesByConnectionIdQueryHandler : IRequestHandler<GetMessagesByConnectionIdQueryRequest, GetMessagesByConnectionIdQueryResponse>
{
    private readonly IMessageReadRepository _messageReadRepository;

    public GetMessagesByConnectionIdQueryHandler(IMessageReadRepository messageReadRepository)
    {
        _messageReadRepository = messageReadRepository;
    }

    public async Task<GetMessagesByConnectionIdQueryResponse> Handle(GetMessagesByConnectionIdQueryRequest request, CancellationToken cancellationToken)
    {
        var paginatedResult = await _messageReadRepository
            .GetWhere(m => m.ConnectionId == request.ConnectionId, tracking: false)
            .Include(m => m.Sender)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(m.Id, m.ConnectionId, m.SenderId, m.Content, m.IsRead, m.CreatedAt))
            .ToPaginatedListAsync(
                request.Pagination.Page,
                request.Pagination.Size,
                cancellationToken);

        return new GetMessagesByConnectionIdQueryResponse(paginatedResult);
    }
}
