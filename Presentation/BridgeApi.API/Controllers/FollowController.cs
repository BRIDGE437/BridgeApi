using BridgeApi.Application.Features.Commands.Follow.FollowUser;
using BridgeApi.Application.Features.Commands.Follow.UnfollowUser;
using BridgeApi.Application.Features.Queries.Follow.GetFollowers;
using BridgeApi.Application.Features.Queries.Follow.GetFollowing;
using BridgeApi.Application.Features.Queries.Follow.IsFollowing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class FollowController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Follow([FromBody] FollowUserCommandRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Unfollow(
        [FromQuery] string followerId,
        [FromQuery] string followingId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnfollowUserCommandRequest(followerId, followingId), cancellationToken);
        return NoContent();
    }

    [HttpGet("following/{userId}")]
    public async Task<IActionResult> GetFollowing(string userId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetFollowingQueryRequest(userId), cancellationToken);
        return Ok(response);
    }

    [HttpGet("followers/{userId}")]
    public async Task<IActionResult> GetFollowers(string userId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetFollowersQueryRequest(userId), cancellationToken);
        return Ok(response);
    }

    [HttpGet("is-following")]
    public async Task<IActionResult> IsFollowing(
        [FromQuery] string followerId,
        [FromQuery] string followingId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new IsFollowingQueryRequest(followerId, followingId), cancellationToken);
        return Ok(response);
    }
}
