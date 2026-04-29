namespace BridgeApi.Application.Features.Commands.Auth.ResetPassword;

public record ResetPasswordCommandResponse(bool Success, IReadOnlyList<string>? Errors);
