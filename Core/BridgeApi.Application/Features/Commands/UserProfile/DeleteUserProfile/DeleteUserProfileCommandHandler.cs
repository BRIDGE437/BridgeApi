using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using MediatR;

namespace BridgeApi.Application.Features.Commands.UserProfile.DeleteUserProfile;

public class DeleteUserProfileCommandHandler : IRequestHandler<DeleteUserProfileCommandRequest, DeleteUserProfileCommandResponse?>
{
    private readonly IUserProfileWriteRepository _userProfileWriteRepository;

    public DeleteUserProfileCommandHandler(IUserProfileWriteRepository userProfileWriteRepository)
    {
        _userProfileWriteRepository = userProfileWriteRepository;
    }

    public async Task<DeleteUserProfileCommandResponse?> Handle(DeleteUserProfileCommandRequest request, CancellationToken cancellationToken)
    {
        var removed = await _userProfileWriteRepository.RemoveAsync(request.Id);
        if (!removed)
            return null;

        await _userProfileWriteRepository.SaveAsync();
        return new DeleteUserProfileCommandResponse();
    }
}
