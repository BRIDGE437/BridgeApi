using BridgeApi.Application.Abstractions.Repositories.FounderProfile;
using BridgeApi.Application.Abstractions.Repositories.InvestorProfile;
using BridgeApi.Application.Abstractions.Repositories.TalentProfile;
using BridgeApi.Application.Abstractions.Repositories.UserProfile;
using BridgeApi.Application.Abstractions.Services;
using BridgeApi.Application.Exceptions;
using BridgeApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Features.Commands.Admin.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommandRequest, ChangeUserRoleCommandResponse>
{
    private readonly IAuthService _authService;
    private readonly IUserProfileReadRepository _userProfileReadRepository;
    private readonly IFounderProfileReadRepository _founderReadRepository;
    private readonly IFounderProfileWriteRepository _founderWriteRepository;
    private readonly IInvestorProfileReadRepository _investorReadRepository;
    private readonly IInvestorProfileWriteRepository _investorWriteRepository;
    private readonly ITalentProfileReadRepository _talentReadRepository;
    private readonly ITalentProfileWriteRepository _talentWriteRepository;
    private readonly ILogger<ChangeUserRoleCommandHandler> _logger;

    public ChangeUserRoleCommandHandler(
        IAuthService authService,
        IUserProfileReadRepository userProfileReadRepository,
        IFounderProfileReadRepository founderReadRepository,
        IFounderProfileWriteRepository founderWriteRepository,
        IInvestorProfileReadRepository investorReadRepository,
        IInvestorProfileWriteRepository investorWriteRepository,
        ITalentProfileReadRepository talentReadRepository,
        ITalentProfileWriteRepository talentWriteRepository,
        ILogger<ChangeUserRoleCommandHandler> logger)
    {
        _authService = authService;
        _userProfileReadRepository = userProfileReadRepository;
        _founderReadRepository = founderReadRepository;
        _founderWriteRepository = founderWriteRepository;
        _investorReadRepository = investorReadRepository;
        _investorWriteRepository = investorWriteRepository;
        _talentReadRepository = talentReadRepository;
        _talentWriteRepository = talentWriteRepository;
        _logger = logger;
    }

    public async Task<ChangeUserRoleCommandResponse> Handle(ChangeUserRoleCommandRequest request, CancellationToken cancellationToken)
    {
        var profile = await _userProfileReadRepository
            .GetWhere(p => p.UserId == request.UserId, tracking: false)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User profile not found.");

        if (request.NewRole != UserRole.Founder)
        {
            var founder = await _founderReadRepository.GetWhere(f => f.UserProfileId == profile.Id, tracking: true).FirstOrDefaultAsync(cancellationToken);
            if (founder is not null) await _founderWriteRepository.RemoveAsync(founder);
        }
        if (request.NewRole != UserRole.Investor)
        {
            var investor = await _investorReadRepository.GetWhere(i => i.UserProfileId == profile.Id, tracking: true).FirstOrDefaultAsync(cancellationToken);
            if (investor is not null) await _investorWriteRepository.RemoveAsync(investor);
        }
        if (request.NewRole != UserRole.Talent)
        {
            var talent = await _talentReadRepository.GetWhere(t => t.UserProfileId == profile.Id, tracking: true).FirstOrDefaultAsync(cancellationToken);
            if (talent is not null) await _talentWriteRepository.RemoveAsync(talent);
        }

        await _founderWriteRepository.SaveAsync();
        await _investorWriteRepository.SaveAsync();
        await _talentWriteRepository.SaveAsync();

        var success = await _authService.ChangeUserRoleAsync(request.UserId, request.NewRole, cancellationToken);

        _logger.LogInformation("Admin changed user {UserId} role to {NewRole}", request.UserId, request.NewRole);

        return new ChangeUserRoleCommandResponse(request.UserId, request.NewRole, success);
    }
}
