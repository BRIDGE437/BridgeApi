using BridgeApi.Application.Abstractions.Repositories.Connection;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Connection.RejectConnection;

public class RejectConnectionCommandHandler : IRequestHandler<RejectConnectionCommandRequest, RejectConnectionCommandResponse?>
{
    private readonly IConnectionReadRepository _connectionReadRepository;
    private readonly IConnectionWriteRepository _connectionWriteRepository;
    private readonly ILogger<RejectConnectionCommandHandler> _logger;

    public RejectConnectionCommandHandler(
        IConnectionReadRepository connectionReadRepository,
        IConnectionWriteRepository connectionWriteRepository,
        ILogger<RejectConnectionCommandHandler> logger)
    {
        _connectionReadRepository = connectionReadRepository;
        _connectionWriteRepository = connectionWriteRepository;
        _logger = logger;
    }

    public async Task<RejectConnectionCommandResponse?> Handle(RejectConnectionCommandRequest request, CancellationToken cancellationToken)
    {
        var connection = await _connectionReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (connection == null)
            return null;

        if (connection.ReceiverId != request.RequestingUserId)
            throw new UnauthorizedAccessException("You do not have permission to reject this connection request.");

        connection.Status = 2; // Rejected

        await _connectionWriteRepository.UpdateAsync(connection);
        await _connectionWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Connection {ConnectionId} rejected",
            connection.Id);

        return new RejectConnectionCommandResponse(
            connection.Id,
            connection.SenderId,
            connection.ReceiverId,
            connection.IntentId,
            connection.Note,
            connection.Status,
            connection.CreatedAt);
    }
}
