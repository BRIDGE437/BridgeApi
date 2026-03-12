using BridgeApi.Application.Features.Commands.Auth.Login;
using BridgeApi.Application.Features.Commands.Auth.RefreshToken;
using BridgeApi.Application.Features.Commands.Auth.Register;
using BridgeApi.Application.Features.Commands.Auth.GoogleLogin;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenCommandResponse>> RefreshToken(
        [FromBody] RefreshTokenCommandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
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
