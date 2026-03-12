namespace BridgeApi.Application.Features.Commands.UserProfile.UpdateUserProfile;

/// <summary>
/// Request body for PUT /api/UserProfile/{id}. Id comes from route.
/// </summary>
public record UpdateUserProfileCommandBody(
    string? Name,
    string? Surname,
    string? Title,
    string? Bio,
    string? Location,
    string? ProfileImage,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? WebsiteUrl);
