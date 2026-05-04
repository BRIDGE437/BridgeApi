using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Features.Commands.Connection.AcceptConnection;
using BridgeApi.Application.Features.Commands.Connection.CreateConnection;
using BridgeApi.Application.Features.Commands.Connection.DeleteConnection;
using BridgeApi.Application.Features.Commands.Connection.RejectConnection;
using BridgeApi.Application.Features.Commands.Connection.UpdateConnection;
using BridgeApi.Application.Features.Queries.Connection.GetAllConnections;
using BridgeApi.Application.Features.Queries.Connection.GetConnectionById;
using BridgeApi.Application.Features.Queries.Message.GetMessagesByConnectionId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConnectionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetAllConnectionsQueryResponse>> GetAll(
        [FromQuery] string? senderId,
        [FromQuery] string? receiverId,
        [FromQuery] int? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetAllConnectionsQueryRequest(senderId, receiverId, status, new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<GetMessagesByConnectionIdQueryResponse>> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetMessagesByConnectionIdQueryRequest(id, new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetConnectionByIdQueryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetConnectionByIdQueryRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateConnectionCommandResponse>> Create(
        [FromBody] CreateConnectionCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateConnectionCommandResponse>> Update(
        Guid id,
        [FromBody] UpdateConnectionCommandBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateConnectionCommandRequest(id, body.Note, body.Status);
        var response = await _mediator.Send(request, cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPut("{id:guid}/accept")]
    public async Task<ActionResult<AcceptConnectionCommandResponse>> Accept(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _mediator.Send(new AcceptConnectionCommandRequest(id, userId), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<ActionResult<RejectConnectionCommandResponse>> Reject(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var response = await _mediator.Send(new RejectConnectionCommandRequest(id, userId), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteConnectionCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return NoContent();
    }
}
