using BridgeApi.Application.Abstractions.Repositories.Follow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Follow.GetFollowing;

public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQueryRequest, GetFollowingQueryResponse>
{
    private readonly IFollowReadRepository _followReadRepository;

    public GetFollowingQueryHandler(IFollowReadRepository followReadRepository)
    {
        _followReadRepository = followReadRepository;
    }

    public async Task<GetFollowingQueryResponse> Handle(GetFollowingQueryRequest request, CancellationToken cancellationToken)
    {
        var users = await _followReadRepository.GetAll(tracking: false)
            .Where(f => f.FollowerId == request.UserId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FollowUserDto(
                f.Following.Id,
                f.Following.UserName,
                f.Following.UserProfile != null
                    ? ((f.Following.UserProfile.Name ?? "") + " " + (f.Following.UserProfile.Surname ?? "")).Trim()
                    : null,
                f.Following.UserProfile != null ? f.Following.UserProfile.Title : null,
                f.Following.UserProfile != null ? f.Following.UserProfile.Headline : null,
                f.Following.UserProfile != null ? f.Following.UserProfile.ProfileImage : null,
                f.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetFollowingQueryResponse(users);
    }
}
