using BridgeApi.Application.Features.Queries.Follow.GetFollowing;

namespace BridgeApi.Application.Features.Queries.Follow.GetFollowers;

public record GetFollowersQueryResponse(IReadOnlyList<FollowUserDto> Users);
