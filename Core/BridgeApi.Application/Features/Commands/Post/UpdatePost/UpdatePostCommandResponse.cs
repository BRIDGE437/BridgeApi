namespace BridgeApi.Application.Features.Commands.Post.UpdatePost;

public record UpdatePostCommandResponse(
    Guid Id,
    string UserId,
    string? Content,
    string? ImageUrl,
    int LikeCount,
    int CommentCount,
    DateTime CreatedAt);
