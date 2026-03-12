using MediatR;

namespace BridgeApi.Application.Features.Commands.Post.DeletePost;

public record DeletePostCommandRequest(Guid Id) : IRequest<DeletePostCommandResponse?>;
