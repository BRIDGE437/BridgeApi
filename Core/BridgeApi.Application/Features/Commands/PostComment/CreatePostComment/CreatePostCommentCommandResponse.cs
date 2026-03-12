namespace BridgeApi.Application.Features.Commands.PostComment.CreatePostComment;

public record CreatePostCommentCommandResponse(
    Guid Id,
    Guid PostId,
    string UserId,
    string CommentText,
    DateTime CreatedAt);
