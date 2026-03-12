using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using MediatR;
using UserProfileEntity = BridgeApi.Domain.Entities.UserProfile;

namespace BridgeApi.Application.Features.Commands.UserProfile.CreateUserProfile;

public class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommandRequest, CreateUserProfileCommandResponse>
{
    private readonly IUserProfileWriteRepository _userProfileWriteRepository;

    public CreateUserProfileCommandHandler(IUserProfileWriteRepository userProfileWriteRepository)
    {
        _userProfileWriteRepository = userProfileWriteRepository;
    }

    public async Task<CreateUserProfileCommandResponse> Handle(CreateUserProfileCommandRequest request, CancellationToken cancellationToken)
    {
        var profile = new UserProfileEntity
        {
            UserId = request.UserId,
            Name = request.Name,
            Surname = request.Surname,
            Title = request.Title,
            Bio = request.Bio,
            Location = request.Location,
            ProfileImage = request.ProfileImage,
            PhoneNumber = request.PhoneNumber,
            LinkedInUrl = request.LinkedInUrl,
            GitHubUrl = request.GitHubUrl,
            WebsiteUrl = request.WebsiteUrl
        };

        await _userProfileWriteRepository.AddAsync(profile);
        await _userProfileWriteRepository.SaveAsync();

        return new CreateUserProfileCommandResponse(
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
