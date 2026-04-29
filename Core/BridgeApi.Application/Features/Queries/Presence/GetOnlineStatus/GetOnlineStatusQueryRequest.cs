using MediatR;

namespace BridgeApi.Application.Features.Queries.Presence.GetOnlineStatus;

public record GetOnlineStatusQueryRequest(List<string> UserIds) : IRequest<GetOnlineStatusQueryResponse>;
