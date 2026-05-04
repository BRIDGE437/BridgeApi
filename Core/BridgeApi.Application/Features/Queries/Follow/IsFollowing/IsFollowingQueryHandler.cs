using BridgeApi.Application.Abstractions.Repositories.Follow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Follow.IsFollowing;

public class IsFollowingQueryHandler : IRequestHandler<IsFollowingQueryRequest, IsFollowingQueryResponse>
{
    private readonly IFollowReadRepository _followReadRepository;

    public IsFollowingQueryHandler(IFollowReadRepository followReadRepository)
    {
        _followReadRepository = followReadRepository;
    }

    public async Task<IsFollowingQueryResponse> Handle(IsFollowingQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _followReadRepository.GetAll(tracking: false);

        var forward = await query.AnyAsync(
            f => f.FollowerId == request.FollowerId && f.FollowingId == request.FollowingId,
            cancellationToken);

        var reverse = await query.AnyAsync(
            f => f.FollowerId == request.FollowingId && f.FollowingId == request.FollowerId,
            cancellationToken);

        return new IsFollowingQueryResponse(forward, forward && reverse);
    }
}
