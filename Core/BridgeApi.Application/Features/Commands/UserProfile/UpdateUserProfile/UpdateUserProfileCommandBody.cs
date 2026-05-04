namespace BridgeApi.Application.Features.Commands.UserProfile.UpdateUserProfile;

public record UpdateUserProfileCommandBody(
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
    string? WebsiteUrl);
