namespace BridgeApi.Application.Features.Commands.Intent.UpdateIntent;

public record UpdateIntentCommandBody(string? Title, string? Description, bool? IsActive);
