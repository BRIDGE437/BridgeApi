namespace BridgeApi.Application.Features.Queries.UserProfile.GetProfileCompletion;

public record GetProfileCompletionQueryResponse(
    int Percentage,
    string[] MissingFields,
    bool IsComplete,
    string? Role);
