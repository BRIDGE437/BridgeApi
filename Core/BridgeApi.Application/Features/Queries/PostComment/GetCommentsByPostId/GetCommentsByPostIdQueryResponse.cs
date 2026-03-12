using BridgeApi.Application.Abstractions.Pagination;

namespace BridgeApi.Application.Features.Queries.PostComment.GetCommentsByPostId;

public record GetCommentsByPostIdQueryResponse(PaginatedResponse<PostCommentDto> Data);

public record PostCommentDto(
    Guid Id,
    Guid PostId,
    string UserId,
    string CommentText,
    DateTime CreatedAt);
