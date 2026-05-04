using MediatR;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetProfileCompletion;

public record GetProfileCompletionQueryRequest(string UserId) : IRequest<GetProfileCompletionQueryResponse>;
