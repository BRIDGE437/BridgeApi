using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos.Profiles;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetUserProfileByUserId;

public class GetUserProfileByUserIdQueryHandler : IRequestHandler<GetUserProfileByUserIdQueryRequest, GetUserProfileByUserIdQueryResponse?>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;
    private readonly IUserRoleService _userRoleService;

    public GetUserProfileByUserIdQueryHandler(
        IUserProfileReadRepository userProfileReadRepository,
        IUserRoleService userRoleService)
    {
        _userProfileReadRepository = userProfileReadRepository;
        _userRoleService = userRoleService;
    }

    public async Task<GetUserProfileByUserIdQueryResponse?> Handle(GetUserProfileByUserIdQueryRequest request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileReadRepository
            .GetWhere(p => p.UserId == request.UserId, tracking: false)
            .Include(p => p.FounderProfile)
            .Include(p => p.InvestorProfile)
            .Include(p => p.TalentProfile)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
            return null;

        var role = await _userRoleService.GetPrimaryRoleAsync(request.UserId);

        FounderProfileDto? founderDto = null;
        if (profile.FounderProfile is not null)
        {
            var f = profile.FounderProfile;
            founderDto = new FounderProfileDto(
                f.Id, f.UserProfileId, f.StartupName, f.StartupWebsite, f.Stage, f.PrimarySector,
                f.SecondarySectors, f.FundingNeedUsd, f.TeamSize, f.PitchDeckUrl, f.OneLiner,
                f.ProblemStatement, f.FoundedYear, f.Country, f.IsActivelyFundraising);
        }

        InvestorProfileDto? investorDto = null;
        if (profile.InvestorProfile is not null)
        {
            var i = profile.InvestorProfile;
            investorDto = new InvestorProfileDto(
                i.Id, i.UserProfileId, i.FirmName, i.FirmWebsite, i.CheckSizeMinUsd, i.CheckSizeMaxUsd,
                i.PreferredStages, i.PreferredSectors, i.PreferredGeographies, i.PortfolioCompanyCount,
                i.InvestmentThesis, i.IsAcceptingPitches);
        }

        TalentProfileDto? talentDto = null;
        if (profile.TalentProfile is not null)
        {
            var t = profile.TalentProfile;
            talentDto = new TalentProfileDto(
                t.Id, t.UserProfileId, t.Headline, t.Skills, t.LookingFor, t.WorkPreference,
                t.YearsOfExperience, t.ExpectedSalaryMonthlyUsd, t.OpenToWork, t.AvailableFrom,
                t.CurrentRole, t.CurrentCompany, t.InterestedSectors);
        }

        return new GetUserProfileByUserIdQueryResponse(
            profile.Id, profile.UserId, profile.Name, profile.Surname, profile.Title,
            profile.Headline, profile.Bio, profile.Location, profile.ProfileImage, profile.CoverImage,
            profile.PhoneNumber, profile.LinkedInUrl, profile.GitHubUrl, profile.WebsiteUrl,
            profile.OnboardingCompletedAt, profile.CreatedAt,
            role, founderDto, investorDto, talentDto);
    }
}
