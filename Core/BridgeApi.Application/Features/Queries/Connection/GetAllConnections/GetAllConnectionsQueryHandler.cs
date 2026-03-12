using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Abstractions.Repositories.Connection;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Connection.GetAllConnections;

public class GetAllConnectionsQueryHandler : IRequestHandler<GetAllConnectionsQueryRequest, GetAllConnectionsQueryResponse>
{
    private readonly IConnectionReadRepository _connectionReadRepository;

    public GetAllConnectionsQueryHandler(IConnectionReadRepository connectionReadRepository)
    {
        _connectionReadRepository = connectionReadRepository;
    }

    public async Task<GetAllConnectionsQueryResponse> Handle(GetAllConnectionsQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _connectionReadRepository.GetAll(tracking: false);

        if (request.SenderId != null)
            query = query.Where(c => c.SenderId == request.SenderId);
        if (request.ReceiverId != null)
            query = query.Where(c => c.ReceiverId == request.ReceiverId);
        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        var paginatedResult = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConnectionDto(c.Id, c.SenderId, c.ReceiverId, c.IntentId, c.Note, c.Status, c.CreatedAt))
            .ToPaginatedListAsync(
                request.Pagination.Page,
                request.Pagination.Size,
                cancellationToken);

        return new GetAllConnectionsQueryResponse(paginatedResult);
    }
}
