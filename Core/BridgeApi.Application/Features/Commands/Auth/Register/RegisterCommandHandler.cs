using BridgeApi.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommandRequest, RegisterCommandResponse>
{
    private readonly IAuthService _authService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthService authService,
        ILogger<RegisterCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<RegisterCommandResponse> Handle(RegisterCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password, request.Role);

        return new RegisterCommandResponse(result.Id, result.Username, result.Email, result.Role, result.Token);
    }
}
