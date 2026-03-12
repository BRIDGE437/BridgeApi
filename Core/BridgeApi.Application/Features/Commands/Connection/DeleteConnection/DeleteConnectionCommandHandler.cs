using BridgeApi.Application.Abstractions.Repositories.Connection;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Connection.DeleteConnection;

public class DeleteConnectionCommandHandler : IRequestHandler<DeleteConnectionCommandRequest, DeleteConnectionCommandResponse?>
{
    private readonly IConnectionWriteRepository _connectionWriteRepository;

    public DeleteConnectionCommandHandler(IConnectionWriteRepository connectionWriteRepository)
    {
        _connectionWriteRepository = connectionWriteRepository;
    }

    public async Task<DeleteConnectionCommandResponse?> Handle(DeleteConnectionCommandRequest request, CancellationToken cancellationToken)
    {
        var removed = await _connectionWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _connectionWriteRepository.SaveAsync();
        return new DeleteConnectionCommandResponse();
    }
}
