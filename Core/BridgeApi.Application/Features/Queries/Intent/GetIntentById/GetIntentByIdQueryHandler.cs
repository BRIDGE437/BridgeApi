using BridgeApi.Application.Abstractions.Repositories.Intent;
using MediatR;

namespace BridgeApi.Application.Features.Queries.Intent.GetIntentById;

public class GetIntentByIdQueryHandler : IRequestHandler<GetIntentByIdQueryRequest, GetIntentByIdQueryResponse?>
{
    private readonly IIntentReadRepository _intentReadRepository;

    public GetIntentByIdQueryHandler(IIntentReadRepository intentReadRepository)
    {
        _intentReadRepository = intentReadRepository;
    }

    public async Task<GetIntentByIdQueryResponse?> Handle(GetIntentByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var intent = await _intentReadRepository.GetByIdAsync(request.Id, tracking: false);
        if (intent == null)
            return null;

        return new GetIntentByIdQueryResponse(intent.Id, intent.Title, intent.Description, intent.IsActive, intent.CreatedAt);
    }
}
