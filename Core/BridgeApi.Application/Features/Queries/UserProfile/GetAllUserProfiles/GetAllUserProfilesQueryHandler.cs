using BridgeApi.Application.Abstractions.Pagination;
using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetAllUserProfiles;

public class GetAllUserProfilesQueryHandler : IRequestHandler<GetAllUserProfilesQueryRequest, GetAllUserProfilesQueryResponse>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;

    public GetAllUserProfilesQueryHandler(IUserProfileReadRepository userProfileReadRepository)
    {
        _userProfileReadRepository = userProfileReadRepository;
    }

    public async Task<GetAllUserProfilesQueryResponse> Handle(GetAllUserProfilesQueryRequest request, CancellationToken cancellationToken)
    {
        var paginatedResult = await _userProfileReadRepository
            .GetAll(tracking: false)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new UserProfileDto(
                p.Id,
                p.UserId,
                p.Name,
                p.Surname,
                p.Title,
                p.Bio,
                p.Location,
                p.ProfileImage,
                p.PhoneNumber,
                p.LinkedInUrl,
                p.GitHubUrl,
                p.WebsiteUrl,
                p.CreatedAt))
            .ToPaginatedListAsync(
                request.Pagination.Page,
                request.Pagination.Size,
                cancellationToken);

        return new GetAllUserProfilesQueryResponse(paginatedResult);
    }
}
