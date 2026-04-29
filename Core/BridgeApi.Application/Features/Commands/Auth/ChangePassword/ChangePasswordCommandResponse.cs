namespace BridgeApi.Application.Features.Commands.Auth.ChangePassword;

public record ChangePasswordCommandResponse(bool Success, IReadOnlyList<string>? Errors);
