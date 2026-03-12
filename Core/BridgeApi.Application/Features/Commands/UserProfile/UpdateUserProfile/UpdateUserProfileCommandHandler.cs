using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using MediatR;

namespace BridgeApi.Application.Features.Commands.UserProfile.UpdateUserProfile;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommandRequest, UpdateUserProfileCommandResponse?>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;
    private readonly IUserProfileWriteRepository _userProfileWriteRepository;

    public UpdateUserProfileCommandHandler(
        IUserProfileReadRepository userProfileReadRepository,
        IUserProfileWriteRepository userProfileWriteRepository)
    {
        _userProfileReadRepository = userProfileReadRepository;
        _userProfileWriteRepository = userProfileWriteRepository;
    }

    public async Task<UpdateUserProfileCommandResponse?> Handle(UpdateUserProfileCommandRequest request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileReadRepository.GetByIdAsync(request.Id, tracking: true);
        if (profile == null)
            return null;

        if (request.Name != null) profile.Name = request.Name;
        if (request.Surname != null) profile.Surname = request.Surname;
        if (request.Title != null) profile.Title = request.Title;
        if (request.Bio != null) profile.Bio = request.Bio;
        if (request.Location != null) profile.Location = request.Location;
        if (request.ProfileImage != null) profile.ProfileImage = request.ProfileImage;
        if (request.PhoneNumber != null) profile.PhoneNumber = request.PhoneNumber;
        if (request.LinkedInUrl != null) profile.LinkedInUrl = request.LinkedInUrl;
        if (request.GitHubUrl != null) profile.GitHubUrl = request.GitHubUrl;
        if (request.WebsiteUrl != null) profile.WebsiteUrl = request.WebsiteUrl;

        await _userProfileWriteRepository.UpdateAsync(profile);
        await _userProfileWriteRepository.SaveAsync();

        return new UpdateUserProfileCommandResponse(
            profile.Id,
            profile.UserId,
            profile.Name,
            profile.Surname,
            profile.Title,
            profile.Bio,
            profile.Location,
            profile.ProfileImage,
            profile.PhoneNumber,
            profile.LinkedInUrl,
            profile.GitHubUrl,
            profile.WebsiteUrl,
            profile.CreatedAt);
    }
}
