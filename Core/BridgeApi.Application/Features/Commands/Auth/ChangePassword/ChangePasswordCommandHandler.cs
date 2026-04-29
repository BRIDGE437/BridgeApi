using BridgeApi.Application.Abstractions.Services;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommandRequest, ChangePasswordCommandResponse>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ChangePasswordCommandResponse> Handle(ChangePasswordCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword, cancellationToken);
        return new ChangePasswordCommandResponse(result.Success, result.Errors);
    }
}
