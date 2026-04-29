using MediatR;

namespace BridgeApi.Application.Features.Commands.Auth.ResetPassword;

public record ResetPasswordCommandRequest(
    string Email,
    string Token,
    string NewPassword) : IRequest<ResetPasswordCommandResponse>;
