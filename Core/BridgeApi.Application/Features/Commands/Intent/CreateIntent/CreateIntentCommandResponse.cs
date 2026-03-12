namespace BridgeApi.Application.Features.Commands.Intent.CreateIntent;

public record CreateIntentCommandResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsActive,
    DateTime CreatedAt);
