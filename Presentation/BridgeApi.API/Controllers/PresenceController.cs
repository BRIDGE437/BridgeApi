using BridgeApi.Application.Features.Queries.Presence.GetOnlineStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class PresenceController : ControllerBase
{
    private readonly IMediator _mediator;

    public PresenceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("online-status")]
    public async Task<ActionResult<GetOnlineStatusQueryResponse>> GetOnlineStatus(
        [FromBody] GetOnlineStatusQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
