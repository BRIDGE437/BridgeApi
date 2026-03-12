using MediatR;

namespace BridgeApi.Application.Features.Commands.UserIntent.AddUserIntent;

public record AddUserIntentCommandRequest(string UserId, Guid IntentId) : IRequest<AddUserIntentCommandResponse>;
