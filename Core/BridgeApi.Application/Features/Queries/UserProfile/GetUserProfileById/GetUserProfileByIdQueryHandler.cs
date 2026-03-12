using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using MediatR;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileById;

public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQueryRequest, GetUserProfileByIdQueryResponse?>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;

    public GetUserProfileByIdQueryHandler(IUserProfileReadRepository userProfileReadRepository)
    {
        _userProfileReadRepository = userProfileReadRepository;
    }

    public async Task<GetUserProfileByIdQueryResponse?> Handle(GetUserProfileByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileReadRepository.GetByIdAsync(request.Id, tracking: false);
        if (profile == null)
            return null;

        return new GetUserProfileByIdQueryResponse(
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
