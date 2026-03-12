using BridgeApi.Application.Abstractions.Repositories.Post;
using MediatR;
using PostEntity = BridgeApi.Domain.Entities.Post;

namespace BridgeApi.Application.Features.Commands.Post.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommandRequest, UpdatePostCommandResponse?>
{
    private readonly IPostReadRepository _postReadRepository;
    private readonly IPostWriteRepository _postWriteRepository;

    public UpdatePostCommandHandler(
        IPostReadRepository postReadRepository,
        IPostWriteRepository postWriteRepository)
    {
        _postReadRepository = postReadRepository;
        _postWriteRepository = postWriteRepository;
    }

    public async Task<UpdatePostCommandResponse?> Handle(UpdatePostCommandRequest request, CancellationToken cancellationToken)
    {
        var post = await _postReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (post == null)
            return null;

        if (request.Content != null) post.Content = request.Content;
        if (request.ImageUrl != null) post.ImageUrl = request.ImageUrl;

        await _postWriteRepository.UpdateAsync(post);
        await _postWriteRepository.SaveAsync();

        return new UpdatePostCommandResponse(
            post.Id,
            post.UserId,
            post.Content,
            post.ImageUrl,
            post.LikeCount,
            post.CommentCount,
            post.CreatedAt);
    }
}
