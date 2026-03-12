using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Features.Commands.User.CreateUser;
using BridgeApi.Application.Features.Commands.User.DeleteUser;
using BridgeApi.Application.Features.Commands.User.UpdateUser;
using BridgeApi.Application.Features.Queries.User.GetAllUsers;
using BridgeApi.Application.Features.Queries.User.GetUserById;
using BridgeApi.Application.Features.Queries.UserIntent.GetUserIntents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<GetAllUsersQueryResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetAllUsersQueryRequest(new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{userId}/intents")]
    public async Task<ActionResult<GetUserIntentsQueryResponse>> GetIntents(string userId, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserIntentsQueryRequest(userId), cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserByIdQueryResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserByIdQueryRequest(id), cancellationToken);
        if (response == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserCommandResponse>> Create(
        [FromBody] CreateUserCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateUserCommandResponse>> Update(
        string id,
        [FromBody] UpdateUserCommandBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateUserCommandRequest(id, body.Username, body.Email, body.Role);
        var response = await _mediator.Send(request, cancellationToken);
        if (response == null)
            return NotFound();

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteUserCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();

        return NoContent();
    }
}
