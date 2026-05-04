using BridgeApi.Application.Abstractions.Repositories.FounderProfile;
using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos.Profiles;
using BridgeApi.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FounderProfileEntity = BridgeApi.Domain.Entities.FounderProfile;

namespace BridgeApi.Application.Features.Commands.FounderProfile.Upsert;

public class UpsertFounderProfileCommandHandler : IRequestHandler<UpsertFounderProfileCommandRequest, FounderProfileDto>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;
    private readonly IFounderProfileReadRepository _founderProfileReadRepository;
    private readonly IFounderProfileWriteRepository _founderProfileWriteRepository;
    private readonly IUserRoleService _userRoleService;
    private readonly ILogger<UpsertFounderProfileCommandHandler> _logger;

    public UpsertFounderProfileCommandHandler(
        IUserProfileReadRepository userProfileReadRepository,
        IFounderProfileReadRepository founderProfileReadRepository,
        IFounderProfileWriteRepository founderProfileWriteRepository,
        IUserRoleService userRoleService,
        ILogger<UpsertFounderProfileCommandHandler> logger)
    {
        _userProfileReadRepository = userProfileReadRepository;
        _founderProfileReadRepository = founderProfileReadRepository;
        _founderProfileWriteRepository = founderProfileWriteRepository;
        _userRoleService = userRoleService;
        _logger = logger;
    }

    public async Task<FounderProfileDto> Handle(UpsertFounderProfileCommandRequest request, CancellationToken cancellationToken)
    {
        if (!await _userRoleService.IsInRoleAsync(request.UserId, "Founder"))
            throw new UnauthorizedAccessException("Only users with Founder role can upsert a founder profile.");

        var userProfile = await _userProfileReadRepository
            .GetWhere(p => p.UserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User profile not found for current user.");

        var existing = await _founderProfileReadRepository
            .GetWhere(f => f.UserProfileId == userProfile.Id, tracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            existing = new FounderProfileEntity
            {
                UserProfileId = userProfile.Id,
                StartupName = request.StartupName,
                StartupWebsite = request.StartupWebsite,
                Stage = request.Stage,
                PrimarySector = request.PrimarySector,
                SecondarySectors = request.SecondarySectors ?? Array.Empty<Domain.Enums.Sector>(),
                FundingNeedUsd = request.FundingNeedUsd,
                TeamSize = request.TeamSize,
                PitchDeckUrl = request.PitchDeckUrl,
                OneLiner = request.OneLiner,
                ProblemStatement = request.ProblemStatement,
                FoundedYear = request.FoundedYear,
                Country = request.Country,
                IsActivelyFundraising = request.IsActivelyFundraising
            };
            await _founderProfileWriteRepository.AddAsync(existing);
        }
        else
        {
            existing.StartupName = request.StartupName;
            existing.StartupWebsite = request.StartupWebsite;
            existing.Stage = request.Stage;
            existing.PrimarySector = request.PrimarySector;
            existing.SecondarySectors = request.SecondarySectors ?? Array.Empty<Domain.Enums.Sector>();
            existing.FundingNeedUsd = request.FundingNeedUsd;
            existing.TeamSize = request.TeamSize;
            existing.PitchDeckUrl = request.PitchDeckUrl;
            existing.OneLiner = request.OneLiner;
            existing.ProblemStatement = request.ProblemStatement;
            existing.FoundedYear = request.FoundedYear;
            existing.Country = request.Country;
            existing.IsActivelyFundraising = request.IsActivelyFundraising;
            await _founderProfileWriteRepository.UpdateAsync(existing);
        }

        await _founderProfileWriteRepository.SaveAsync();
        _logger.LogInformation("Founder profile upserted for user {UserId}", request.UserId);

        return new FounderProfileDto(
            existing.Id, existing.UserProfileId, existing.StartupName, existing.StartupWebsite,
            existing.Stage, existing.PrimarySector, existing.SecondarySectors, existing.FundingNeedUsd,
            existing.TeamSize, existing.PitchDeckUrl, existing.OneLiner, existing.ProblemStatement,
            existing.FoundedYear, existing.Country, existing.IsActivelyFundraising);
    }
}
