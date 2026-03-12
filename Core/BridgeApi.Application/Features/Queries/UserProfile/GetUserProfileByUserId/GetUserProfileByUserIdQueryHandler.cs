using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;

public class GetUserProfileByUserIdQueryHandler : IRequestHandler<GetUserProfileByUserIdQueryRequest, GetUserProfileByUserIdQueryResponse?>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;

    public GetUserProfileByUserIdQueryHandler(IUserProfileReadRepository userProfileReadRepository)
    {
        _userProfileReadRepository = userProfileReadRepository;
    }

    public async Task<GetUserProfileByUserIdQueryResponse?> Handle(GetUserProfileByUserIdQueryRequest request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileReadRepository
            .GetWhere(p => p.UserId == request.UserId, tracking: false)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
            return null;

        return new GetUserProfileByUserIdQueryResponse(
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
