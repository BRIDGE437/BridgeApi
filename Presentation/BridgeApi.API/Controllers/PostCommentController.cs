using BridgeApi.Application.Features.Commands.PostComment.CreatePostComment;
using BridgeApi.Application.Features.Commands.PostComment.DeletePostComment;
using BridgeApi.Application.Features.Queries.PostComment.GetCommentsByPostId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class PostCommentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostCommentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetCommentsByPostIdQueryResponse>> GetByPostId(
        [FromQuery] Guid postId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCommentsByPostIdQueryRequest(postId), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreatePostCommentCommandResponse>> Create(
        [FromBody] CreatePostCommentCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetByPostId),
            new { postId = response.PostId },
            response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeletePostCommentCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return NoContent();
    }
}
