using MediatR;

namespace BridgeApi.Application.Features.Queries.Follow.GetFollowers;

public record GetFollowersQueryRequest(string UserId) : IRequest<GetFollowersQueryResponse>;
