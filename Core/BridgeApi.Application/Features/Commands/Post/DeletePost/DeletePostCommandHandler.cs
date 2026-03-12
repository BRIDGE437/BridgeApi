using BridgeApi.Application.Abstractions.Repositories.Post;
using MediatR;

namespace BridgeApi.Application.Features.Commands.Post.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommandRequest, DeletePostCommandResponse?>
{
    private readonly IPostWriteRepository _postWriteRepository;

    public DeletePostCommandHandler(IPostWriteRepository postWriteRepository)
    {
        _postWriteRepository = postWriteRepository;
    }

    public async Task<DeletePostCommandResponse?> Handle(DeletePostCommandRequest request, CancellationToken cancellationToken)
    {
        var removed = await _postWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _postWriteRepository.SaveAsync();
        return new DeletePostCommandResponse();
    }
}
