using BridgeApi.Application.Abstractions.Services;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommandRequest, ForgotPasswordCommandResponse>
{
    private const string NeutralMessage = "Eğer bu e-posta sistemde kayıtlıysa, sıfırlama bağlantısı gönderildi.";

    private readonly IAuthService _authService;

    public ForgotPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ForgotPasswordCommandResponse> Handle(ForgotPasswordCommandRequest request, CancellationToken cancellationToken)
    {
        await _authService.RequestPasswordResetAsync(request.Email, cancellationToken);
        return new ForgotPasswordCommandResponse(true, NeutralMessage);
    }
}
