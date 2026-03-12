using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Features.Commands.UserProfile.CreateUserProfile;
using BridgeApi.Application.Features.Commands.UserProfile.DeleteUserProfile;
using BridgeApi.Application.Features.Commands.UserProfile.UpdateUserProfile;
using BridgeApi.Application.Features.Queries.UserProfile.GetAllUserProfiles;
using BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileById;
using BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetAllUserProfilesQueryResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetAllUserProfilesQueryRequest(new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUserProfileByIdQueryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserProfileByIdQueryRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<GetUserProfileByUserIdQueryResponse>> GetByUserId(string userId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserProfileByUserIdQueryRequest(userId), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserProfileCommandResponse>> Create(
        [FromBody] CreateUserProfileCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateUserProfileCommandResponse>> Update(
        Guid id,
        [FromBody] UpdateUserProfileCommandBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateUserProfileCommandRequest(
            id,
            body.Name,
            body.Surname,
            body.Title,
            body.Bio,
            body.Location,
            body.ProfileImage,
            body.PhoneNumber,
            body.LinkedInUrl,
            body.GitHubUrl,
            body.WebsiteUrl);
        var response = await _mediator.Send(request, cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteUserProfileCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return NoContent();
    }
}
