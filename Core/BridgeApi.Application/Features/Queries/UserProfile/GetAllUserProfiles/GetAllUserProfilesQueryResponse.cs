using BridgeApi.Application.Abstractions.Pagination;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetAllUserProfiles;

public record GetAllUserProfilesQueryResponse(PaginatedResponse<UserProfileDto> Data);

public record UserProfileDto(
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
