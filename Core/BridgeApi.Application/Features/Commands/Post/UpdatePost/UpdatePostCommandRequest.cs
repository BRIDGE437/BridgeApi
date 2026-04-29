using MediatR;

namespace BridgeApi.Application.Features.Commands.Post.UpdatePost;

public record UpdatePostCommandRequest(
    Guid Id,
    string? Content,
    string? ImageUrl,
    string RequestingUserId,
    bool IsAdmin) : IRequest<UpdatePostCommandResponse?>;
