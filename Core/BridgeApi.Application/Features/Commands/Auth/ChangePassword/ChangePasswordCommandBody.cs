namespace BridgeApi.Application.Features.Commands.Auth.ChangePassword;

public record ChangePasswordCommandBody(string CurrentPassword, string NewPassword);
