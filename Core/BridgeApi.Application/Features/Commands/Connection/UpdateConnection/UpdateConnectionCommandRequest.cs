using MediatR;

namespace BridgeApi.Application.Features.Commands.Connection.UpdateConnection;

public record UpdateConnectionCommandRequest(
    Guid Id,
    string? Note,
    int? Status) : IRequest<UpdateConnectionCommandResponse?>;
