using System.Security.Claims;
using BridgeApi.Application.Features.Commands.File.DeleteFile;
using BridgeApi.Application.Features.Commands.File.UploadFile;
using BridgeApi.Application.Features.Queries.File.GetFileById;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IMediator _mediator;

    public FileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<UploadFileCommandResponse>> Upload(
        IFormFile file,
        [FromQuery] FileCategory category,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        await using var stream = file.OpenReadStream();
        var request = new UploadFileCommandRequest(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            category,
            userId);

        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetFileByIdQueryResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetFileByIdQueryRequest(id), cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");

        var response = await _mediator.Send(
            new DeleteFileCommandRequest(id, userId, isAdmin),
            cancellationToken);

        if (response == null)
            return NotFound();
        return NoContent();
    }
}
