using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.InvestorProfile.Upsert;

public record UpsertInvestorProfileCommandBody(
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
