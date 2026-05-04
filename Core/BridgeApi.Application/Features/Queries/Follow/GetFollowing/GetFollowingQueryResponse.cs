namespace BridgeApi.Application.Features.Queries.Follow.GetFollowing;

public record GetFollowingQueryResponse(IReadOnlyList<FollowUserDto> Users);

public record FollowUserDto(
    string Id,
    string? UserName,
    string? FullName,
    string? Title,
    string? Headline,
    string? ProfileImage,
    DateTime FollowedAt);
