using MediatR;

namespace BridgeApi.Application.Features.Commands.Follow.UnfollowUser;

public record UnfollowUserCommandRequest(string FollowerId, string FollowingId) : IRequest<UnfollowUserCommandResponse>;
