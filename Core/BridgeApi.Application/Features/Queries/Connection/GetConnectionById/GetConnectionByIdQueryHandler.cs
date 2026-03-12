using BridgeApi.Application.Abstractions.Repositories.Connection;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Connection.GetConnectionById;

public class GetConnectionByIdQueryHandler : IRequestHandler<GetConnectionByIdQueryRequest, GetConnectionByIdQueryResponse?>
{
    private readonly IConnectionReadRepository _connectionReadRepository;

    public GetConnectionByIdQueryHandler(IConnectionReadRepository connectionReadRepository)
    {
        _connectionReadRepository = connectionReadRepository;
    }

    public async Task<GetConnectionByIdQueryResponse?> Handle(GetConnectionByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var connection = await _connectionReadRepository
            .GetAll(tracking: false)
            .Include(c => c.Sender)
            .Include(c => c.Receiver)
            .Include(c => c.Intent)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (connection == null)
            return null;

        return new GetConnectionByIdQueryResponse(
            connection.Id,
            connection.SenderId,
            connection.ReceiverId,
            connection.IntentId,
            connection.Note,
            connection.Status,
            connection.CreatedAt);
    }
}
