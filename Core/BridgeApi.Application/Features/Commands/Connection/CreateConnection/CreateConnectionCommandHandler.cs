using BridgeApi.Application.Abstractions.Repositories.Connection;
using MediatR;
using Microsoft.Extensions.Logging;
using ConnectionEntity = BridgeApi.Domain.Entities.Connection;

namespace BridgeApi.Application.Features.Commands.Connection.CreateConnection;

public class CreateConnectionCommandHandler : IRequestHandler<CreateConnectionCommandRequest, CreateConnectionCommandResponse>
{
    private readonly IConnectionWriteRepository _connectionWriteRepository;
    private readonly ILogger<CreateConnectionCommandHandler> _logger;

    public CreateConnectionCommandHandler(
        IConnectionWriteRepository connectionWriteRepository,
        ILogger<CreateConnectionCommandHandler> logger)
    {
        _connectionWriteRepository = connectionWriteRepository;
        _logger = logger;
    }

    public async Task<CreateConnectionCommandResponse> Handle(CreateConnectionCommandRequest request, CancellationToken cancellationToken)
    {
        var connection = new ConnectionEntity
        {
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            IntentId = request.IntentId,
            Note = request.Note,
            Status = 0 // Pending
        };

        await _connectionWriteRepository.AddAsync(connection);
        await _connectionWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Connection request created: {SenderId} -> {ReceiverId} (Intent: {IntentId})",
            request.SenderId,
            request.ReceiverId,
            request.IntentId);

        return new CreateConnectionCommandResponse(
            connection.Id,
            connection.SenderId,
            connection.ReceiverId,
            connection.IntentId,
            connection.Note,
            connection.Status,
            connection.CreatedAt);
    }
}
