using MediatR;

namespace BridgeApi.Application.Features.Commands.Connection.CreateConnection;

public record CreateConnectionCommandRequest(
    string SenderId,
    string ReceiverId,
    Guid IntentId,
    string? Note) : IRequest<CreateConnectionCommandResponse>;
