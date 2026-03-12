using MediatR;

namespace BridgeApi.Application.Features.Commands.PostLike.LikePost;

public record LikePostCommandRequest(Guid PostId, string UserId) : IRequest<LikePostCommandResponse>;
