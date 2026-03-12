using BridgeApi.Application.Abstractions.Repositories.Follow;
using BridgeApi.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using FollowEntity = BridgeApi.Domain.Entities.Follow;

namespace BridgeApi.Application.Features.Commands.Follow.FollowUser;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommandRequest, FollowUserCommandResponse>
{
    private readonly IFollowWriteRepository _followWriteRepository;
    private readonly ILogger<FollowUserCommandHandler> _logger;

    public FollowUserCommandHandler(
        IFollowWriteRepository followWriteRepository,
        ILogger<FollowUserCommandHandler> logger)
    {
        _followWriteRepository = followWriteRepository;
        _logger = logger;
    }

    public async Task<FollowUserCommandResponse> Handle(FollowUserCommandRequest request, CancellationToken cancellationToken)
    {
        var follow = new FollowEntity
        {
            FollowerId = request.FollowerId,
            FollowingId = request.FollowingId
        };

        await _followWriteRepository.AddAsync(follow);
        await _followWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {FollowerId} followed User {FollowingId}",
            request.FollowerId,
            request.FollowingId);

        return new FollowUserCommandResponse();
    }
}
