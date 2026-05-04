using MediatR;

namespace BridgeApi.Application.Features.Commands.Message.DeleteMessage;

public record DeleteMessageCommandRequest(Guid Id, string RequestingUserId, bool IsAdmin) : IRequest<DeleteMessageCommandResponse?>;
