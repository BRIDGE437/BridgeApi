namespace BridgeApi.Application.Features.Queries.Presence.GetOnlineStatus;

public record GetOnlineStatusQueryResponse(Dictionary<string, bool> Statuses);
