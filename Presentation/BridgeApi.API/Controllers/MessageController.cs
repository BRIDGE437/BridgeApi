using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Features.Commands.Message.CreateMessage;
using BridgeApi.Application.Features.Commands.Message.DeleteMessage;
using BridgeApi.Application.Features.Commands.Message.MarkMessageAsRead;
using BridgeApi.Application.Features.Queries.Message.GetMessagesByConnectionId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetMessagesByConnectionIdQueryResponse>> GetByConnectionId(
        [FromQuery] Guid connectionId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetMessagesByConnectionIdQueryRequest(connectionId, new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateMessageCommandResponse>> Create(
        [FromBody] CreateMessageCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetByConnectionId),
            new { connectionId = response.ConnectionId },
            response);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<MarkMessageAsReadCommandResponse>> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new MarkMessageAsReadCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteMessageCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return NoContent();
    }
}
