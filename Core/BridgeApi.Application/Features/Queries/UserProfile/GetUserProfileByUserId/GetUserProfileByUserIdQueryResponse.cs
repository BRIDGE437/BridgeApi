using BridgeApi.Application.Dtos.Profiles;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;

public record GetUserProfileByUserIdQueryResponse(
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
    DateTime? OnboardingCompletedAt,
    DateTime CreatedAt,
    string? Role,
    FounderProfileDto? FounderProfile,
    InvestorProfileDto? InvestorProfile,
    TalentProfileDto? TalentProfile);
