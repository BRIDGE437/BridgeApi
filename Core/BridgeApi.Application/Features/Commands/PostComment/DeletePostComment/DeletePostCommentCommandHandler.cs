using BridgeApi.Application.Abstractions.Repositories.Post;
using BridgeApi.Application.Abstractions.Repositories.PostComment;
using MediatR;

namespace BridgeApi.Application.Features.Commands.PostComment.DeletePostComment;

public class DeletePostCommentCommandHandler : IRequestHandler<DeletePostCommentCommandRequest, DeletePostCommentCommandResponse?>
{
    private readonly IPostCommentReadRepository _postCommentReadRepository;
    private readonly IPostCommentWriteRepository _postCommentWriteRepository;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;

    public DeletePostCommentCommandHandler(
        IPostCommentReadRepository postCommentReadRepository,
        IPostCommentWriteRepository postCommentWriteRepository,
        IPostReadRepository postReadRepository,
        IPostWriteRepository postWriteRepository)
    {
        _postCommentReadRepository = postCommentReadRepository;
        _postCommentWriteRepository = postCommentWriteRepository;
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
    }

    public async Task<DeletePostCommentCommandResponse?> Handle(DeletePostCommentCommandRequest request, CancellationToken cancellationToken)
    {
        var comment = await _postCommentReadRepository.GetByIdAsync(request.Id, tracking: false);
        if (comment == null)
            return null;

        var postId = comment.PostId;

        var removed = await _postCommentWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _postCommentWriteRepository.SaveAsync();

        var post = await _postReadRepository.GetByIdAsync(postId, tracking: true);
        if (post != null && post.CommentCount > 0)
        {
            post.CommentCount--;
            await _postWriteRepository.UpdateAsync(post);
            await _postWriteRepository.SaveAsync();
        }

        return new DeletePostCommentCommandResponse();
    }
}
