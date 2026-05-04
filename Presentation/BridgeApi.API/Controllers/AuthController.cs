using BridgeApi.Application.Features.Commands.Auth.ChangePassword;
using BridgeApi.Application.Features.Commands.Auth.ForgotPassword;
using BridgeApi.Application.Features.Commands.Auth.Login;
using BridgeApi.Application.Features.Commands.Auth.RefreshToken;
using BridgeApi.Application.Features.Commands.Auth.Register;
using BridgeApi.Application.Features.Commands.Auth.GoogleLogin;
using BridgeApi.Application.Features.Commands.Auth.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BridgeApi.API.Controllers;

[EnableRateLimiting("auth")]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public AuthController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterCommandResponse>> Register(
        [FromBody] RegisterCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginCommandResponse>> Login(
        [FromBody] LoginCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<GoogleLoginCommandResponse>> GoogleLogin(
        [FromBody] GoogleLoginCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("password-reset")]
    public async Task<ActionResult<ForgotPasswordCommandResponse>> ForgotPassword(
        [FromBody] ForgotPasswordCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("password-reset")]
    public async Task<ActionResult<ResetPasswordCommandResponse>> ResetPassword(
        [FromBody] ResetPasswordCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenCommandResponse>> RefreshToken(
        [FromBody] RefreshTokenCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ChangePasswordCommandResponse>> ChangePassword(
        [FromBody] ChangePasswordCommandBody body,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var request = new ChangePasswordCommandRequest(userId, body.CurrentPassword, body.NewPassword);
        var response = await _mediator.Send(request, cancellationToken);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Sadece Development ortamında çalışır. Hızlı test için token üretir.
    /// Kullanım: POST /api/Auth/fast-login?usernameOrEmail=testuser&password=Test1234!
    /// </summary>
    [DisableRateLimiting]
    [HttpPost("fast-login")]
    public async Task<IActionResult> FastLogin(
        [FromQuery] string usernameOrEmail = "admin",
        [FromQuery] string password = "Admin123!",
        CancellationToken cancellationToken = default)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var response = await _mediator.Send(
            new LoginCommandRequest(usernameOrEmail, password),
            cancellationToken);

        return Ok(response);
    }
}
