using MediatR;

namespace BridgeApi.Application.Features.Commands.Intent.CreateIntent;

public record CreateIntentCommandRequest(
    string Title,
    string? Description,
    bool IsActive = true) : IRequest<CreateIntentCommandResponse>;
