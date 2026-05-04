using MediatR;

namespace BridgeApi.Application.Features.Commands.Connection.AcceptConnection;

public record AcceptConnectionCommandRequest(Guid Id, string RequestingUserId) : IRequest<AcceptConnectionCommandResponse?>;
