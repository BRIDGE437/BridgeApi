using BridgeApi.Application.Features.Commands.Follow.FollowUser;
using BridgeApi.Application.Features.Commands.Follow.UnfollowUser;
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
}
