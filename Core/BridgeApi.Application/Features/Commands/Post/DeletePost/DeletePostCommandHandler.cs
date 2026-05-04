using BridgeApi.Application.Abstractions.Repositories.Post;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Post.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommandRequest, DeletePostCommandResponse?>
{
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;

    public DeletePostCommandHandler(IPostReadRepository postReadRepository, IPostWriteRepository postWriteRepository)
    {
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
    }

    public async Task<DeletePostCommandResponse?> Handle(DeletePostCommandRequest request, CancellationToken cancellationToken)
    {
        var post = await _postReadRepository.GetByIdAsync(request.Id, tracking: false);
        if (post == null)
            return null;

        if (post.UserId != request.RequestingUserId && !request.IsAdmin)
            throw new UnauthorizedAccessException("You do not have permission to delete this post.");

        var removed = await _postWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _postWriteRepository.SaveAsync();
        return new DeletePostCommandResponse();
    }
}
