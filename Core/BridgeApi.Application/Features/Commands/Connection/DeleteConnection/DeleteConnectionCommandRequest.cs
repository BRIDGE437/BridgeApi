using MediatR;

namespace BridgeApi.Application.Features.Commands.Connection.DeleteConnection;

public record DeleteConnectionCommandRequest(Guid Id) : IRequest<DeleteConnectionCommandResponse?>;
