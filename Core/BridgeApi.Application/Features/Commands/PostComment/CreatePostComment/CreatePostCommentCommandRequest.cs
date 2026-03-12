using MediatR;

namespace BridgeApi.Application.Features.Commands.PostComment.CreatePostComment;

public record CreatePostCommentCommandRequest(
    Guid PostId,
    string UserId,
    string CommentText) : IRequest<CreatePostCommentCommandResponse>;
