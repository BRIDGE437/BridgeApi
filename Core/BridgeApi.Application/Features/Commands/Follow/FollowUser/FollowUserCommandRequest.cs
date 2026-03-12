using MediatR;

namespace BridgeApi.Application.Features.Commands.Follow.FollowUser;

public record FollowUserCommandRequest(string FollowerId, string FollowingId) : IRequest<FollowUserCommandResponse>;
