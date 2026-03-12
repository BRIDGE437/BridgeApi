using BridgeApi.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommandRequest, LoginCommandResponse>
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthService authService,
        ILogger<LoginCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<LoginCommandResponse> Handle(LoginCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request.UsernameOrEmail, request.Password);

        return new LoginCommandResponse(result.Id, result.Username, result.Email, result.Token);
    }
}
