using BridgeApi.Application.Features.Commands.Admin.ChangeUserRole;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize(Roles = "Admin")]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult<ChangeUserRoleCommandResponse>> ChangeUserRole(
        string userId,
        [FromBody] ChangeUserRoleCommandBody body,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ChangeUserRoleCommandRequest(userId, body.NewRole), cancellationToken);
        return Ok(response);
    }
}
