using MediatR;

namespace BridgeApi.Application.Features.Commands.Message.CreateMessage;

public record CreateMessageCommandRequest(
    Guid ConnectionId,
    string SenderId,
    string Content) : IRequest<CreateMessageCommandResponse>;
