using BridgeApi.Application.Abstractions.Repositories.Follow;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Follow.UnfollowUser;

public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommandRequest, UnfollowUserCommandResponse>
{
    private readonly IFollowReadRepository _followReadRepository;
    private readonly IFollowWriteRepository _followWriteRepository;
    private readonly ILogger<UnfollowUserCommandHandler> _logger;

    public UnfollowUserCommandHandler(
        IFollowReadRepository followReadRepository,
        IFollowWriteRepository followWriteRepository,
        ILogger<UnfollowUserCommandHandler> logger)
    {
        _followReadRepository = followReadRepository;
        _followWriteRepository = followWriteRepository;
        _logger = logger;
    }

    public async Task<UnfollowUserCommandResponse> Handle(UnfollowUserCommandRequest request, CancellationToken cancellationToken)
    {
        var follow = await _followReadRepository
            .GetWhere(f => f.FollowerId == request.FollowerId && f.FollowingId == request.FollowingId, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        if (follow != null)
        {
            await _followWriteRepository.RemoveAsync(follow);
            await _followWriteRepository.SaveAsync();

            _logger.LogInformation(
                "User {FollowerId} unfollowed User {FollowingId}",
                request.FollowerId,
                request.FollowingId);
        }

        return new UnfollowUserCommandResponse();
    }
}
