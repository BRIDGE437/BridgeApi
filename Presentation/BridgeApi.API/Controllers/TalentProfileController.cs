using BridgeApi.Application.Features.Commands.TalentProfile.Upsert;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BridgeApi.API.Controllers;

[Authorize(Roles = "Talent")]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class TalentProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public TalentProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertTalentProfileCommandBody body,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var request = new UpsertTalentProfileCommandRequest(
            userId, body.Headline, body.Skills, body.LookingFor, body.WorkPreference,
            body.YearsOfExperience, body.ExpectedSalaryMonthlyUsd, body.OpenToWork,
            body.AvailableFrom, body.CurrentRole, body.CurrentCompany, body.InterestedSectors);
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
