using MediatR;

namespace BridgeApi.Application.Features.Commands.Post.UpdatePost;

public record UpdatePostCommandRequest(
    Guid Id,
    string? Content,
    string? ImageUrl) : IRequest<UpdatePostCommandResponse?>;
