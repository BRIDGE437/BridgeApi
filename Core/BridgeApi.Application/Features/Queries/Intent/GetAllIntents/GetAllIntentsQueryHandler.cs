using BridgeApi.Application.Abstractions.Repositories.Intent;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Intent.GetAllIntents;

public class GetAllIntentsQueryHandler : IRequestHandler<GetAllIntentsQueryRequest, GetAllIntentsQueryResponse>
{
    private readonly IIntentReadRepository _intentReadRepository;

    public GetAllIntentsQueryHandler(IIntentReadRepository intentReadRepository)
    {
        _intentReadRepository = intentReadRepository;
    }

    public async Task<GetAllIntentsQueryResponse> Handle(GetAllIntentsQueryRequest request, CancellationToken cancellationToken)
    {
        var intents = await _intentReadRepository
            .GetAll(tracking: false)
            .Select(i => new IntentDto(i.Id, i.Title, i.Description, i.IsActive, i.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetAllIntentsQueryResponse(intents);
    }
}
