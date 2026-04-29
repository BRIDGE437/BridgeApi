using BridgeApi.Application.Abstractions.Repositories.InvestorProfile;
using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Dtos.Profiles;
using BridgeApi.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InvestorProfileEntity = BridgeApi.Domain.Entities.InvestorProfile;

namespace BridgeApi.Application.Features.Commands.InvestorProfile.Upsert;

public class UpsertInvestorProfileCommandHandler : IRequestHandler<UpsertInvestorProfileCommandRequest, InvestorProfileDto>
{
    private readonly IUserProfileReadRepository _userProfileReadRepository;
    private readonly IInvestorProfileReadRepository _investorProfileReadRepository;
    private readonly IInvestorProfileWriteRepository _investorProfileWriteRepository;
    private readonly IUserRoleService _userRoleService;
    private readonly ILogger<UpsertInvestorProfileCommandHandler> _logger;

    public UpsertInvestorProfileCommandHandler(
        IUserProfileReadRepository userProfileReadRepository,
        IInvestorProfileReadRepository investorProfileReadRepository,
        IInvestorProfileWriteRepository investorProfileWriteRepository,
        IUserRoleService userRoleService,
        ILogger<UpsertInvestorProfileCommandHandler> logger)
    {
        _userProfileReadRepository = userProfileReadRepository;
        _investorProfileReadRepository = investorProfileReadRepository;
        _investorProfileWriteRepository = investorProfileWriteRepository;
        _userRoleService = userRoleService;
        _logger = logger;
    }

    public async Task<InvestorProfileDto> Handle(UpsertInvestorProfileCommandRequest request, CancellationToken cancellationToken)
    {
        if (!await _userRoleService.IsInRoleAsync(request.UserId, "Investor"))
            throw new UnauthorizedAccessException("Only users with Investor role can upsert an investor profile.");

        var userProfile = await _userProfileReadRepository
            .GetWhere(p => p.UserId == request.UserId, tracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User profile not found for current user.");

        var existing = await _investorProfileReadRepository
            .GetWhere(i => i.UserProfileId == userProfile.Id, tracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            existing = new InvestorProfileEntity
            {
                UserProfileId = userProfile.Id,
                FirmName = request.FirmName,
                FirmWebsite = request.FirmWebsite,
                CheckSizeMinUsd = request.CheckSizeMinUsd,
                CheckSizeMaxUsd = request.CheckSizeMaxUsd,
                PreferredStages = request.PreferredStages ?? Array.Empty<Domain.Enums.InvestmentStage>(),
                PreferredSectors = request.PreferredSectors ?? Array.Empty<Domain.Enums.Sector>(),
                PreferredGeographies = request.PreferredGeographies ?? Array.Empty<string>(),
                PortfolioCompanyCount = request.PortfolioCompanyCount,
                InvestmentThesis = request.InvestmentThesis,
                IsAcceptingPitches = request.IsAcceptingPitches
            };
            await _investorProfileWriteRepository.AddAsync(existing);
        }
        else
        {
            existing.FirmName = request.FirmName;
            existing.FirmWebsite = request.FirmWebsite;
            existing.CheckSizeMinUsd = request.CheckSizeMinUsd;
            existing.CheckSizeMaxUsd = request.CheckSizeMaxUsd;
            existing.PreferredStages = request.PreferredStages ?? Array.Empty<Domain.Enums.InvestmentStage>();
            existing.PreferredSectors = request.PreferredSectors ?? Array.Empty<Domain.Enums.Sector>();
            existing.PreferredGeographies = request.PreferredGeographies ?? Array.Empty<string>();
            existing.PortfolioCompanyCount = request.PortfolioCompanyCount;
            existing.InvestmentThesis = request.InvestmentThesis;
            existing.IsAcceptingPitches = request.IsAcceptingPitches;
            await _investorProfileWriteRepository.UpdateAsync(existing);
        }

        await _investorProfileWriteRepository.SaveAsync();
        _logger.LogInformation("Investor profile upserted for user {UserId}", request.UserId);

        return new InvestorProfileDto(
            existing.Id, existing.UserProfileId, existing.FirmName, existing.FirmWebsite,
            existing.CheckSizeMinUsd, existing.CheckSizeMaxUsd, existing.PreferredStages,
            existing.PreferredSectors, existing.PreferredGeographies, existing.PortfolioCompanyCount,
            existing.InvestmentThesis, existing.IsAcceptingPitches);
    }
}
