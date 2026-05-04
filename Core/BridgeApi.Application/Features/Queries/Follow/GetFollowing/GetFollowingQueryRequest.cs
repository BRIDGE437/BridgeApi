using MediatR;

namespace BridgeApi.Application.Features.Queries.Follow.GetFollowing;

public record GetFollowingQueryRequest(string UserId) : IRequest<GetFollowingQueryResponse>;
