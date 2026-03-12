using BridgeApi.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Auth.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommandRequest, GoogleLoginCommandResponse>
{
    private readonly IAuthService _authService;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    public GoogleLoginCommandHandler(
        IAuthService authService,
        ILogger<GoogleLoginCommandHandler> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<GoogleLoginCommandResponse> Handle(GoogleLoginCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.GoogleLoginAsync(request.IdToken, request.Role);

        return new GoogleLoginCommandResponse(result.Id, result.Username, result.Email, result.Token);
    }
}
