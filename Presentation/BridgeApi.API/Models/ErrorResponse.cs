namespace BridgeApi.API.Models;

public record ValidationErrorResponse(Dictionary<string, string[]> Errors);

public record BadRequestErrorResponse(string Message);

public record InternalErrorResponse(string Title, int Status);

public record RateLimitResponse(string Message, int? RetryAfterSeconds);
