using MediatR;

namespace BridgeApi.Application.Features.Commands.Connection.RejectConnection;

public record RejectConnectionCommandRequest(Guid Id) : IRequest<RejectConnectionCommandResponse?>;
