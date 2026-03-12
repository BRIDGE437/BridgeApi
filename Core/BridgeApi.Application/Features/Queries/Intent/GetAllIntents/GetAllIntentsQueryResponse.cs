namespace BridgeApi.Application.Features.Queries.Intent.GetAllIntents;

public record GetAllIntentsQueryResponse(IReadOnlyList<IntentDto> Intents);

public record IntentDto(Guid Id, string Title, string? Description, bool IsActive, DateTime CreatedAt);
