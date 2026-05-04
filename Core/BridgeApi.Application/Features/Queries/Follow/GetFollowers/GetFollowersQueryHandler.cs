using BridgeApi.Application.Abstractions.Repositories.Follow;
using BridgeApi.Application.Features.Queries.Follow.GetFollowing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.Follow.GetFollowers;

public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQueryRequest, GetFollowersQueryResponse>
{
    private readonly IFollowReadRepository _followReadRepository;

    public GetFollowersQueryHandler(IFollowReadRepository followReadRepository)
    {
        _followReadRepository = followReadRepository;
    }

    public async Task<GetFollowersQueryResponse> Handle(GetFollowersQueryRequest request, CancellationToken cancellationToken)
    {
        var users = await _followReadRepository.GetAll(tracking: false)
            .Where(f => f.FollowingId == request.UserId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FollowUserDto(
                f.Follower.Id,
                f.Follower.UserName,
                f.Follower.UserProfile != null
                    ? ((f.Follower.UserProfile.Name ?? "") + " " + (f.Follower.UserProfile.Surname ?? "")).Trim()
                    : null,
                f.Follower.UserProfile != null ? f.Follower.UserProfile.Title : null,
                f.Follower.UserProfile != null ? f.Follower.UserProfile.Headline : null,
                f.Follower.UserProfile != null ? f.Follower.UserProfile.ProfileImage : null,
                f.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetFollowersQueryResponse(users);
    }
}
