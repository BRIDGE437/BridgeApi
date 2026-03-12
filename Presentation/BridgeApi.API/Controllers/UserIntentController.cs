using BridgeApi.Application.Features.Commands.UserIntent.AddUserIntent;
using BridgeApi.Application.Features.Commands.UserIntent.RemoveUserIntent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class UserIntentController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserIntentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddUserIntentCommandRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Remove([FromQuery] string userId, [FromQuery] Guid intentId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveUserIntentCommandRequest(userId, intentId), cancellationToken);
        return NoContent();
    }
}
