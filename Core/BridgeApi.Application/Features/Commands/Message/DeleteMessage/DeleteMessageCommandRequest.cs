using MediatR;

namespace BridgeApi.Application.Features.Commands.Message.DeleteMessage;

public record DeleteMessageCommandRequest(Guid Id) : IRequest<DeleteMessageCommandResponse?>;
