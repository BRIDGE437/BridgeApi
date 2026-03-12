using BridgeApi.Application.Abstractions.Repositories.Post;
using MediatR;
using Microsoft.Extensions.Logging;
using PostEntity = BridgeApi.Domain.Entities.Post;

namespace BridgeApi.Application.Features.Commands.Post.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommandRequest, CreatePostCommandResponse>
{
    private readonly IPostWriteRepository _postWriteRepository;
    private readonly ILogger<CreatePostCommandHandler> _logger;

    public CreatePostCommandHandler(
        IPostWriteRepository postWriteRepository,
        ILogger<CreatePostCommandHandler> logger)
    {
        _postWriteRepository = postWriteRepository;
        _logger = logger;
    }

    public async Task<CreatePostCommandResponse> Handle(CreatePostCommandRequest request, CancellationToken cancellationToken)
    {
        var post = new PostEntity
        {
            UserId = request.UserId,
            Content = request.Content,
            ImageUrl = request.ImageUrl,
            LikeCount = 0,
            CommentCount = 0
        };

        await _postWriteRepository.AddAsync(post);
        await _postWriteRepository.SaveAsync();

        _logger.LogInformation(
            "User {UserId} created Post",
            request.UserId);

        return new CreatePostCommandResponse(
            post.Id,
            post.UserId,
            post.Content,
            post.ImageUrl,
            post.LikeCount,
            post.CommentCount,
            post.CreatedAt);
    }
}
