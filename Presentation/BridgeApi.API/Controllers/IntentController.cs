using BridgeApi.Application.Features.Commands.Intent.CreateIntent;
using BridgeApi.Application.Features.Commands.Intent.DeleteIntent;
using BridgeApi.Application.Features.Commands.Intent.UpdateIntent;
using BridgeApi.Application.Features.Queries.Intent.GetAllIntents;
using BridgeApi.Application.Features.Queries.Intent.GetIntentById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class IntentController : ControllerBase
{
    private readonly IMediator _mediator;

    public IntentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetAllIntentsQueryResponse>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllIntentsQueryRequest(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetIntentByIdQueryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetIntentByIdQueryRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateIntentCommandResponse>> Create(
        [FromBody] CreateIntentCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateIntentCommandResponse>> Update(
        Guid id,
        [FromBody] UpdateIntentCommandBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateIntentCommandRequest(id, body.Title, body.Description, body.IsActive);
        var response = await _mediator.Send(request, cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeleteIntentCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return NoContent();
    }
}
