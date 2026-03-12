using BridgeApi.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommandRequest, RefreshTokenCommandResponse>
{
    private readonly IAuthService _authService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IAuthService authService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommandRequest request, CancellationToken cancellationToken)
    {
        var token = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);

        return new RefreshTokenCommandResponse(token);
    }
}
