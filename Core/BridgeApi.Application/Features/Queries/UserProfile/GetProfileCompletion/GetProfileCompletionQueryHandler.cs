using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BridgeApi.Application.Features.Queries.UserProfile.GetProfileCompletion;

public class GetProfileCompletionQueryHandler : IRequestHandler<GetProfileCompletionQueryRequest, GetProfileCompletionQueryResponse>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;
    private readonly IUserRoleService _userRoleService;

    public GetProfileCompletionQueryHandler(
        IUserProfileReadRepository userProfileReadRepository,
        IUserRoleService userRoleService)
    {
        _userProfileReadRepository = userProfileReadRepository;
        _userRoleService = userRoleService;
    }

    public async Task<GetProfileCompletionQueryResponse> Handle(GetProfileCompletionQueryRequest request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileReadRepository
            .GetWhere(p => p.UserId == request.UserId, tracking: false)
            .Include(p => p.FounderProfile)
            .Include(p => p.InvestorProfile)
            .Include(p => p.TalentProfile)
            .FirstOrDefaultAsync(cancellationToken);

        var role = await _userRoleService.GetPrimaryRoleAsync(request.UserId);

        if (profile is null)
            return new GetProfileCompletionQueryResponse(0, new[] { "profile" }, false, role);

        var missing = new List<string>();

        // Common fields (4 critical)
        if (string.IsNullOrWhiteSpace(profile.Name)) missing.Add("name");
        if (string.IsNullOrWhiteSpace(profile.Headline)) missing.Add("headline");
        if (string.IsNullOrWhiteSpace(profile.Bio)) missing.Add("bio");
        if (string.IsNullOrWhiteSpace(profile.ProfileImage)) missing.Add("profileImage");

        var totalRequired = 4;

        if (role == "Founder")
        {
            totalRequired += 4;
            var f = profile.FounderProfile;
            if (f is null || string.IsNullOrWhiteSpace(f.StartupName)) missing.Add("startupName");
            if (f is null) { missing.Add("stage"); missing.Add("primarySector"); }
            if (f is null || string.IsNullOrWhiteSpace(f.OneLiner)) missing.Add("oneLiner");
        }
        else if (role == "Investor")
        {
            totalRequired += 3;
            var i = profile.InvestorProfile;
            if (i is null || (i.CheckSizeMinUsd == 0 && i.CheckSizeMaxUsd == 0)) missing.Add("checkSize");
            if (i is null || i.PreferredStages.Length == 0) missing.Add("preferredStages");
            if (i is null || string.IsNullOrWhiteSpace(i.InvestmentThesis)) missing.Add("investmentThesis");
        }
        else if (role == "Talent")
        {
            totalRequired += 3;
            var t = profile.TalentProfile;
            if (t is null || string.IsNullOrWhiteSpace(t.Headline)) missing.Add("talentHeadline");
            if (t is null || t.Skills.Length == 0) missing.Add("skills");
            if (t is null || t.YearsOfExperience <= 0) missing.Add("yearsOfExperience");
        }

        var filled = totalRequired - missing.Count;
        var percentage = totalRequired == 0 ? 100 : (int)Math.Round(100.0 * filled / totalRequired);
        var isComplete = percentage >= 80 && missing.Count == 0;

        return new GetProfileCompletionQueryResponse(percentage, missing.ToArray(), isComplete, role);
    }
}
