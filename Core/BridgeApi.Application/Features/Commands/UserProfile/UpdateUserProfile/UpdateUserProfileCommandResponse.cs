namespace BridgeApi.Application.Features.Commands.UserProfile.UpdateUserProfile;

public record UpdateUserProfileCommandResponse(
    Guid Id,
    string UserId,
    string? Name,
    string? Surname,
    string? Title,
    string? Headline,
    string? Bio,
    string? Location,
    string? ProfileImage,
    string? CoverImage,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? WebsiteUrl,
    DateTime CreatedAt);
