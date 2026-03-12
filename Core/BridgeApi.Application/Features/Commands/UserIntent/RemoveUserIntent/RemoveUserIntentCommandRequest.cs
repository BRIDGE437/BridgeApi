using MediatR;

namespace BridgeApi.Application.Features.Commands.UserIntent.RemoveUserIntent;

public record RemoveUserIntentCommandRequest(string UserId, Guid IntentId) : IRequest<RemoveUserIntentCommandResponse>;
