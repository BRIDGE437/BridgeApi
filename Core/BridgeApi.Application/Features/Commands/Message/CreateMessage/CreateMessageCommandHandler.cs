using BridgeApi.Application.Abstractions.Repositories.Message;
using MediatR;
using Microsoft.Extensions.Logging;
using MessageEntity = BridgeApi.Domain.Entities.Message;

namespace BridgeApi.Application.Features.Commands.Message.CreateMessage;

public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommandRequest, CreateMessageCommandResponse>
{
    private readonly IMessageWriteRepository _messageWriteRepository;
    private readonly ILogger<CreateMessageCommandHandler> _logger;

    public CreateMessageCommandHandler(
        IMessageWriteRepository messageWriteRepository,
        ILogger<CreateMessageCommandHandler> logger)
    {
        _messageWriteRepository = messageWriteRepository;
        _logger = logger;
    }

    public async Task<CreateMessageCommandResponse> Handle(CreateMessageCommandRequest request, CancellationToken cancellationToken)
    {
        var message = new MessageEntity
        {
            ConnectionId = request.ConnectionId,
            SenderId = request.SenderId,
            Content = request.Content,
            IsRead = false
        };

        await _messageWriteRepository.AddAsync(message);
        await _messageWriteRepository.SaveAsync();

        _logger.LogInformation(
            "Message sent in Connection {ConnectionId} by User {SenderId}",
            request.ConnectionId,
            request.SenderId);

        return new CreateMessageCommandResponse(
            message.Id,
            message.ConnectionId,
            message.SenderId,
            message.Content,
            message.IsRead,
            message.CreatedAt);
    }
}
