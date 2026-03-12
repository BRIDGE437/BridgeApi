namespace BridgeApi.Application.Features.Commands.PostLike.LikePost;

/// <summary>
/// Request body for POST /api/Post/{postId}/like. PostId comes from route.
/// </summary>
public record LikePostCommandBody(string UserId);
