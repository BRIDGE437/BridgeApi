using BridgeApi.Application.Abstractions.Repositories.Connection;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Connection.AcceptConnection;

public class AcceptConnectionCommandHandler : IRequestHandler<AcceptConnectionCommandRequest, AcceptConnectionCommandResponse?>
{
    private readonly IConnectionReadRepository _connectionReadRepository;
    private readonly IConnectionWriteRepository _connectionWriteRepository;
    private readonly ILogger<AcceptConnectionCommandHandler> _logger;

    public AcceptConnectionCommandHandler(
        IConnectionReadRepository connectionReadRepository,
        IConnectionWriteRepository connectionWriteRepository,
        ILogger<AcceptConnectionCommandHandler> logger)
    {
        _connectionReadRepository = connectionReadRepository;
        _connectionWriteRepository = connectionWriteRepository;
        _logger = logger;
    }

    public async Task<AcceptConnectionCommandResponse?> Handle(AcceptConnectionCommandRequest request, CancellationToken cancellationToken)
    {
        var connection = await _connectionReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (connection == null)
            return null;

        connection.Status = 1; // Accepted

        await _connectionWriteRepository.UpdateAsync(connection);
        await _connectionWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Connection {ConnectionId} accepted (Sender: {SenderId}, Receiver: {ReceiverId})",
            connection.Id,
            connection.SenderId,
            connection.ReceiverId);

        return new AcceptConnectionCommandResponse(
            connection.Id,
            connection.SenderId,
            connection.ReceiverId,
            connection.IntentId,
            connection.Note,
            connection.Status,
            connection.CreatedAt);
    }
}
