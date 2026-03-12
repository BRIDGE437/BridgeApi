using BridgeApi.Application.Abstractions.Pagination;

namespace BridgeApi.Application.Features.Queries.Post.GetAllPosts;

public record GetAllPostsQueryResponse(PaginatedResponse<PostDto> Data);

public record PostDto(
    Guid Id,
    string UserId,
    string? Content,
    string? ImageUrl,
    int LikeCount,
    int CommentCount,
    DateTime CreatedAt);
