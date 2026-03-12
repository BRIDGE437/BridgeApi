namespace BridgeApi.Application.Features.Commands.Post.CreatePost;

public record CreatePostCommandResponse(
    Guid Id,
    string UserId,
    string? Content,
    string? ImageUrl,
    int LikeCount,
    int CommentCount,
    DateTime CreatedAt);
