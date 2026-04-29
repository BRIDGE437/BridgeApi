using MediatR;

namespace BridgeApi.Application.Features.Commands.Post.DeletePost;

public record DeletePostCommandRequest(Guid Id, string RequestingUserId, bool IsAdmin) : IRequest<DeletePostCommandResponse?>;
