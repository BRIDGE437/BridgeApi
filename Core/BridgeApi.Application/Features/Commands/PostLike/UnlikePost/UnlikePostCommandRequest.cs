using MediatR;

namespace BridgeApi.Application.Features.Commands.PostLike.UnlikePost;

public record UnlikePostCommandRequest(Guid PostId, string UserId) : IRequest<UnlikePostCommandResponse>;
