using BridgeApi.Application.Abstractions.Repositories.Intent;
using BridgeApi.Application.Abstractions.Repositories.UserIntent;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.UserIntent.GetUserIntents;

public class GetUserIntentsQueryHandler : IRequestHandler<GetUserIntentsQueryRequest, GetUserIntentsQueryResponse>
{
    private readonly IUserIntentReadRepository _userIntentReadRepository;
    private readonly IIntentReadRepository _intentReadRepository;

    public GetUserIntentsQueryHandler(
        IUserIntentReadRepository userIntentReadRepository,
        IIntentReadRepository intentReadRepository)
    {
        _userIntentReadRepository = userIntentReadRepository;
        _intentReadRepository = intentReadRepository;
    }

    public async Task<GetUserIntentsQueryResponse> Handle(GetUserIntentsQueryRequest request, CancellationToken cancellationToken)
    {
        var intentIds = await _userIntentReadRepository
            .GetWhere(ui => ui.UserId == request.UserId, tracking: false)
            .Select(ui => ui.IntentId)
            .ToListAsync(cancellationToken);

        if (intentIds.Count == 0)
            return new GetUserIntentsQueryResponse([]);

        var intents = await _intentReadRepository
            .GetWhere(i => intentIds.Contains(i.Id), tracking: false)
            .Select(i => new IntentDto(i.Id, i.Title, i.Description, i.IsActive))
            .ToListAsync(cancellationToken);

        return new GetUserIntentsQueryResponse(intents);
    }
}
