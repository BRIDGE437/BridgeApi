using BridgeApi.Application.Features.Commands.InvestorProfile.Upsert;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BridgeApi.API.Controllers;

[Authorize(Roles = "Investor")]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class InvestorProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvestorProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertInvestorProfileCommandBody body,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var request = new UpsertInvestorProfileCommandRequest(
            userId, body.FirmName, body.FirmWebsite, body.CheckSizeMinUsd, body.CheckSizeMaxUsd,
            body.PreferredStages, body.PreferredSectors, body.PreferredGeographies,
            body.PortfolioCompanyCount, body.InvestmentThesis, body.IsAcceptingPitches);
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
