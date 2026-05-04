using BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;
using MediatR;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetMyFullProfile;

public class GetMyFullProfileQueryHandler : IRequestHandler<GetMyFullProfileQueryRequest, GetUserProfileByUserIdQueryResponse?>
{
    private readonly IMediator _mediator;

    public GetMyFullProfileQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<GetUserProfileByUserIdQueryResponse?> Handle(GetMyFullProfileQueryRequest request, CancellationToken cancellationToken)
    {
        return _mediator.Send(new GetUserProfileByUserIdQueryRequest(request.UserId), cancellationToken);
    }
}
