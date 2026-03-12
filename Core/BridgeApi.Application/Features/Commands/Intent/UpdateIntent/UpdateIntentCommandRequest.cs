using MediatR;

namespace BridgeApi.Application.Features.Commands.Intent.UpdateIntent;

public record UpdateIntentCommandRequest(
    Guid Id,
    string? Title,
    string? Description,
    bool? IsActive) : IRequest<UpdateIntentCommandResponse?>;
