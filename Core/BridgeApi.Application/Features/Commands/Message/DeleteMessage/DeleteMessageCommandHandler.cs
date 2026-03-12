using BridgeApi.Application.Abstractions.Repositories.Message;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Message.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommandRequest, DeleteMessageCommandResponse?>
{
    private readonly IMessageWriteRepository _messageWriteRepository;

    public DeleteMessageCommandHandler(IMessageWriteRepository messageWriteRepository)
    {
        _messageWriteRepository = messageWriteRepository;
    }

    public async Task<DeleteMessageCommandResponse?> Handle(DeleteMessageCommandRequest request, CancellationToken cancellationToken)
    {
        var removed = await _messageWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _messageWriteRepository.SaveAsync();
        return new DeleteMessageCommandResponse();
    }
}
