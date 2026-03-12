namespace BridgeApi.Application.Features.Commands.Intent.UpdateIntent;

public record UpdateIntentCommandResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);
