using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Features.Commands.Notification.CreateNotification;
using BridgeApi.Application.Features.Commands.Notification.DeleteNotification;
using BridgeApi.Application.Features.Commands.Notification.MarkAllNotificationsAsRead;
using BridgeApi.Application.Features.Commands.Notification.MarkNotificationAsRead;
using BridgeApi.Application.Features.Queries.Notification.GetNotifications;
using BridgeApi.Application.Features.Queries.Notification.GetUnreadNotificationCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BridgeApi.API.Controllers;

[Authorize]
[EnableRateLimiting("user-api")]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetNotificationsQueryResponse>> GetNotifications(
        [FromQuery] string userId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetNotificationsQueryRequest(userId, new PaginationRequest(page, size)),
            cancellationToken);
        return Ok(response);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<GetUnreadNotificationCountQueryResponse>> GetUnreadCount(
        [FromQuery] string userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetUnreadNotificationCountQueryRequest(userId),
            cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<MarkNotificationAsReadCommandResponse>> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new MarkNotificationAsReadCommandRequest(id),
            cancellationToken);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<MarkAllNotificationsAsReadCommandResponse>> MarkAllAsRead(
        [FromQuery] string userId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new MarkAllNotificationsAsReadCommandRequest(userId),
            cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new DeleteNotificationCommandRequest(id),
            cancellationToken);
        if (response == null)
            return NotFound();
        return NoContent();
    }
}
