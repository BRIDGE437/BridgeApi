namespace BridgeApi.Application.Features.Queries.Post.GetPostById;

public record GetPostByIdQueryResponse(
    Guid Id,
    string UserId,
    string? Content,
    string? ImageUrl,
    int LikeCount,
    int CommentCount,
    DateTime CreatedAt);
