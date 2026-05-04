using MediatR;
using BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetMyFullProfile;

public record GetMyFullProfileQueryRequest(string UserId) : IRequest<GetUserProfileByUserIdQueryResponse?>;
