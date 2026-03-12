namespace BridgeApi.Application.Features.Commands.UserProfile.CreateUserProfile;

public record CreateUserProfileCommandResponse(
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
