using BridgeApi.Application.Features.Commands.FounderProfile.Upsert;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BridgeApi.API.Controllers;

[Authorize(Roles = "Founder")]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class FounderProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public FounderProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertFounderProfileCommandBody body,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var request = new UpsertFounderProfileCommandRequest(
            userId, body.StartupName, body.StartupWebsite, body.Stage, body.PrimarySector,
            body.SecondarySectors, body.FundingNeedUsd, body.TeamSize, body.PitchDeckUrl,
            body.OneLiner, body.ProblemStatement, body.FoundedYear, body.Country, body.IsActivelyFundraising);
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
