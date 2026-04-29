using MediatR;

namespace BridgeApi.Application.Features.Queries.Follow.IsFollowing;

public record IsFollowingQueryRequest(string FollowerId, string FollowingId) : IRequest<IsFollowingQueryResponse>;
