using BridgeApi.Application.Abstractions.Services;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Presence.GetOnlineStatus;

public class GetOnlineStatusQueryHandler : IRequestHandler<GetOnlineStatusQueryRequest, GetOnlineStatusQueryResponse>
{
    private readonly IPresenceService _presenceService;

    public GetOnlineStatusQueryHandler(IPresenceService presenceService)
    {
        _presenceService = presenceService;
    }

    public async Task<GetOnlineStatusQueryResponse> Handle(GetOnlineStatusQueryRequest request, CancellationToken cancellationToken)
    {
        var statuses = await _presenceService.GetOnlineStatusesAsync(request.UserIds);
        return new GetOnlineStatusQueryResponse(statuses);
    }
}
