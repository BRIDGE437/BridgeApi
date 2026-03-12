using MediatR;

namespace BridgeApi.Application.Features.Commands.PostComment.DeletePostComment;

public record DeletePostCommentCommandRequest(Guid Id) : IRequest<DeletePostCommentCommandResponse?>;
