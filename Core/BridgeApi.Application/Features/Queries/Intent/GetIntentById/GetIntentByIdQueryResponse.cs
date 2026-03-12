namespace BridgeApi.Application.Features.Queries.Intent.GetIntentById;

public record GetIntentByIdQueryResponse(Guid Id, string Title, string? Description, bool IsActive, DateTime CreatedAt);
