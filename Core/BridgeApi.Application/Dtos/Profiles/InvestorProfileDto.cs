using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Dtos.Profiles;

public record InvestorProfileDto(
    Guid Id,
    Guid UserProfileId,
    string? FirmName,
    string? FirmWebsite,
    decimal CheckSizeMinUsd,
    decimal CheckSizeMaxUsd,
    InvestmentStage[] PreferredStages,
    Sector[] PreferredSectors,
    string[] PreferredGeographies,
    int? PortfolioCompanyCount,
    string? InvestmentThesis,
    bool IsAcceptingPitches);
