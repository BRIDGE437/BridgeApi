using BridgeApi.Application.Abstractions.Repositories.TalentProfile;
using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos.Profiles;
using BridgeApi.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalentProfileEntity = BridgeApi.Domain.Entities.TalentProfile;

namespace BridgeApi.Application.Features.Commands.TalentProfile.Upsert;

public class UpsertTalentProfileCommandHandler : IRequestHandler<UpsertTalentProfileCommandRequest, TalentProfileDto>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;
    private readonly ITalentProfileReadRepository _talentProfileReadRepository;
    private readonly ITalentProfileWriteRepository _talentProfileWriteRepository;
    private readonly IUserRoleService _userRoleService;
    private readonly ILogger<UpsertTalentProfileCommandHandler> _logger;

    public UpsertTalentProfileCommandHandler(
        IUserProfileReadRepository userProfileReadRepository,
        ITalentProfileReadRepository talentProfileReadRepository,
        ITalentProfileWriteRepository talentProfileWriteRepository,
        IUserRoleService userRoleService,
        ILogger<UpsertTalentProfileCommandHandler> logger)
    {
        _userProfileReadRepository = userProfileReadRepository;
        _talentProfileReadRepository = talentProfileReadRepository;
        _talentProfileWriteRepository = talentProfileWriteRepository;
        _userRoleService = userRoleService;
        _logger = logger;
    }

    public async Task<TalentProfileDto> Handle(UpsertTalentProfileCommandRequest request, CancellationToken cancellationToken)
    {
        if (!await _userRoleService.IsInRoleAsync(request.UserId, "Talent"))
            throw new UnauthorizedAccessException("Only users with Talent role can upsert a talent profile.");

        var userProfile = await _userProfileReadRepository
            .GetWhere(p => p.UserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User profile not found for current user.");

        var existing = await _talentProfileReadRepository
            .GetWhere(t => t.UserProfileId == userProfile.Id, tracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            existing = new TalentProfileEntity
            {
                UserProfileId = userProfile.Id,
                Headline = request.Headline,
                Skills = request.Skills ?? Array.Empty<string>(),
                LookingFor = request.LookingFor ?? Array.Empty<Domain.Enums.EmploymentType>(),
                WorkPreference = request.WorkPreference,
                YearsOfExperience = request.YearsOfExperience,
                ExpectedSalaryMonthlyUsd = request.ExpectedSalaryMonthlyUsd,
                OpenToWork = request.OpenToWork,
                AvailableFrom = request.AvailableFrom,
                CurrentRole = request.CurrentRole,
                CurrentCompany = request.CurrentCompany,
                InterestedSectors = request.InterestedSectors ?? Array.Empty<Domain.Enums.Sector>()
            };
            await _talentProfileWriteRepository.AddAsync(existing);
        }
        else
        {
            existing.Headline = request.Headline;
            existing.Skills = request.Skills ?? Array.Empty<string>();
            existing.LookingFor = request.LookingFor ?? Array.Empty<Domain.Enums.EmploymentType>();
            existing.WorkPreference = request.WorkPreference;
            existing.YearsOfExperience = request.YearsOfExperience;
            existing.ExpectedSalaryMonthlyUsd = request.ExpectedSalaryMonthlyUsd;
            existing.OpenToWork = request.OpenToWork;
            existing.AvailableFrom = request.AvailableFrom;
            existing.CurrentRole = request.CurrentRole;
            existing.CurrentCompany = request.CurrentCompany;
            existing.InterestedSectors = request.InterestedSectors ?? Array.Empty<Domain.Enums.Sector>();
            await _talentProfileWriteRepository.UpdateAsync(existing);
        }

        await _talentProfileWriteRepository.SaveAsync();
        _logger.LogInformation("Talent profile upserted for user {UserId}", request.UserId);

        return new TalentProfileDto(
            existing.Id, existing.UserProfileId, existing.Headline, existing.Skills, existing.LookingFor,
            existing.WorkPreference, existing.YearsOfExperience, existing.ExpectedSalaryMonthlyUsd,
            existing.OpenToWork, existing.AvailableFrom, existing.CurrentRole, existing.CurrentCompany,
            existing.InterestedSectors);
    }
}
