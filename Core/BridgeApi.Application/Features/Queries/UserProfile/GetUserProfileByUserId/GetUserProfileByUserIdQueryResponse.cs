namespace BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;

public record GetUserProfileByUserIdQueryResponse(
    Guid Id,
    string UserId,
    string? Name,
    string? Surname,
    string? Title,
    string? Bio,
    string? Location,
    string? ProfileImage,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? WebsiteUrl,
    DateTime CreatedAt);
