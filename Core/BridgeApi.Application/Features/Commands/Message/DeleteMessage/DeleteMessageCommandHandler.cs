using BridgeApi.Application.Abstractions.Repositories.Message;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Message.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommandRequest, DeleteMessageCommandResponse?>
{
    private readonly IMessageReadRepository _messageReadRepository;
    private readonly IMessageWriteRepository _messageWriteRepository;

    public DeleteMessageCommandHandler(IMessageReadRepository messageReadRepository, IMessageWriteRepository messageWriteRepository)
    {
        _messageReadRepository = messageReadRepository;
        _messageWriteRepository = messageWriteRepository;
    }

    public async Task<DeleteMessageCommandResponse?> Handle(DeleteMessageCommandRequest request, CancellationToken cancellationToken)
    {
        var message = await _messageReadRepository.GetByIdAsync(request.Id, tracking: false);
        if (message == null)
            return null;

        if (message.SenderId != request.RequestingUserId && !request.IsAdmin)
            throw new UnauthorizedAccessException("You do not have permission to delete this message.");

        var removed = await _messageWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _messageWriteRepository.SaveAsync();
        return new DeleteMessageCommandResponse();
    }
}
