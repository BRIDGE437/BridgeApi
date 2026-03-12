namespace BridgeApi.Application.Features.Queries.UserIntent.GetUserIntents;

public record GetUserIntentsQueryResponse(IReadOnlyList<IntentDto> Intents);

public record IntentDto(Guid Id, string Title, string? Description, bool IsActive);
