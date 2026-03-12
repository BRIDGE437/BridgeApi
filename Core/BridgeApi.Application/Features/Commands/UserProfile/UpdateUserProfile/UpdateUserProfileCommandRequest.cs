using MediatR;

namespace BridgeApi.Application.Features.Commands.UserProfile.UpdateUserProfile;

public record UpdateUserProfileCommandRequest(
    Guid Id,
    string? Name,
    string? Surname,
    string? Title,
    string? Bio,
    string? Location,
    string? ProfileImage,
    string? PhoneNumber,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? WebsiteUrl) : IRequest<UpdateUserProfileCommandResponse?>;
