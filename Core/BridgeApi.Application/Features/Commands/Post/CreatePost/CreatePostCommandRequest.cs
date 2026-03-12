using MediatR;

namespace BridgeApi.Application.Features.Commands.Post.CreatePost;

public record CreatePostCommandRequest(
    string UserId,
    string? Content,
    string? ImageUrl) : IRequest<CreatePostCommandResponse>;
