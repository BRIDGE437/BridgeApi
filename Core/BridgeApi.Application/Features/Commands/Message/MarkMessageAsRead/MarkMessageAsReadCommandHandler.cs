using BridgeApi.Application.Abstractions.Repositories.Message;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Message.MarkMessageAsRead;

public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommandRequest, MarkMessageAsReadCommandResponse?>
{
    private readonly IMessageReadRepository _messageReadRepository;
    private readonly IMessageWriteRepository _messageWriteRepository;

    public MarkMessageAsReadCommandHandler(
        IMessageReadRepository messageReadRepository,
        IMessageWriteRepository messageWriteRepository)
    {
        _messageReadRepository = messageReadRepository;
        _messageWriteRepository = messageWriteRepository;
    }

    public async Task<MarkMessageAsReadCommandResponse?> Handle(MarkMessageAsReadCommandRequest request, CancellationToken cancellationToken)
    {
        var message = await _messageReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (message == null)
            return null;

        message.IsRead = true;

        await _messageWriteRepository.UpdateAsync(message);
        await _messageWriteRepository.SaveAsync();

        return new MarkMessageAsReadCommandResponse(
            message.Id,
            message.ConnectionId,
            message.SenderId,
            message.Content,
            message.IsRead,
            message.CreatedAt);
    }
}
