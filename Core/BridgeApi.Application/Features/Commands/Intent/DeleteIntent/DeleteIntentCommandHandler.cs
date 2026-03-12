using BridgeApi.Application.Abstractions.Repositories.Intent;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Intent.DeleteIntent;

public class DeleteIntentCommandHandler : IRequestHandler<DeleteIntentCommandRequest, DeleteIntentCommandResponse?>
{
    private readonly IIntentWriteRepository _intentWriteRepository;

    public DeleteIntentCommandHandler(IIntentWriteRepository intentWriteRepository)
    {
        _intentWriteRepository = intentWriteRepository;
    }

    public async Task<DeleteIntentCommandResponse?> Handle(DeleteIntentCommandRequest request, CancellationToken cancellationToken)
    {
        var removed = await _intentWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _intentWriteRepository.SaveAsync();
        return new DeleteIntentCommandResponse();
    }
}
