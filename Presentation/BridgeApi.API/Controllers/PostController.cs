using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Features.Commands.Post.CreatePost;
using BridgeApi.Application.Features.Commands.Post.DeletePost;
using BridgeApi.Application.Features.Commands.Post.UpdatePost;
using BridgeApi.Application.Features.Commands.PostLike.LikePost;
using BridgeApi.Application.Features.Commands.PostLike.UnlikePost;
using BridgeApi.Application.Features.Queries.Post.GetAllPosts;
using BridgeApi.Application.Features.Queries.Post.GetPostById;
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
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<GetAllPostsQueryResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetAllPostsQueryRequest(new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{postId:guid}/comments")]
    public async Task<ActionResult<GetCommentsByPostIdQueryResponse>> GetComments(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetCommentsByPostIdQueryRequest(postId, new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPostByIdQueryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetPostByIdQueryRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreatePostCommandResponse>> Create(
        [FromBody] CreatePostCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdatePostCommandResponse>> Update(
        Guid id,
        [FromBody] UpdatePostCommandBody body,
        CancellationToken cancellationToken)
    {
        var request = new UpdatePostCommandRequest(id, body.Content, body.ImageUrl);
        var response = await _mediator.Send(request, cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new DeletePostCommandRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return NoContent();
    }

    [HttpPost("{postId:guid}/like")]
    public async Task<IActionResult> Like(Guid postId, [FromBody] LikePostCommandBody body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new LikePostCommandRequest(postId, body.UserId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{postId:guid}/like/{userId}")]
    public async Task<IActionResult> Unlike(Guid postId, string userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnlikePostCommandRequest(postId, userId), cancellationToken);
        return NoContent();
    }
}
