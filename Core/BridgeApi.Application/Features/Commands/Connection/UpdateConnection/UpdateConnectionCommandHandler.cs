using BridgeApi.Application.Abstractions.Repositories.Connection;
using MediatR;
using ConnectionEntity = BridgeApi.Domain.Entities.Connection;

namespace BridgeApi.Application.Features.Commands.Connection.UpdateConnection;

public class UpdateConnectionCommandHandler : IRequestHandler<UpdateConnectionCommandRequest, UpdateConnectionCommandResponse?>
{
    private readonly IConnectionReadRepository _connectionReadRepository;
    private readonly IConnectionWriteRepository _connectionWriteRepository;

    public UpdateConnectionCommandHandler(
        IConnectionReadRepository connectionReadRepository,
        IConnectionWriteRepository connectionWriteRepository)
    {
        _connectionReadRepository = connectionReadRepository;
        _connectionWriteRepository = connectionWriteRepository;
    }

    public async Task<UpdateConnectionCommandResponse?> Handle(UpdateConnectionCommandRequest request, CancellationToken cancellationToken)
    {
        var connection = await _connectionReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (connection == null)
            return null;

        if (request.Note != null) connection.Note = request.Note;
        if (request.Status.HasValue) connection.Status = request.Status.Value;

        await _connectionWriteRepository.UpdateAsync(connection);
        await _connectionWriteRepository.SaveAsync();

        return new UpdateConnectionCommandResponse(
            connection.Id,
            connection.SenderId,
            connection.ReceiverId,
            connection.IntentId,
            connection.Note,
            connection.Status,
            connection.CreatedAt);
    }
}
