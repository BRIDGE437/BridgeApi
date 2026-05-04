namespace BridgeApi.Application.Dtos;

public record PasswordResetResult(bool Success, IReadOnlyList<string>? Errors);
