using MediatR;

namespace BridgeApi.Application.Features.Commands.Intent.DeleteIntent;

public record DeleteIntentCommandRequest(Guid Id) : IRequest<DeleteIntentCommandResponse?>;
