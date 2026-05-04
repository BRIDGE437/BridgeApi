using BridgeApi.Application.Dtos.Profiles;
using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.InvestorProfile.Upsert;

public record UpsertInvestorProfileCommandRequest(
    string UserId,
    string? FirmName,
    string? FirmWebsite,
    decimal CheckSizeMinUsd,
    decimal CheckSizeMaxUsd,
    InvestmentStage[] PreferredStages,
    Sector[] PreferredSectors,
    string[] PreferredGeographies,
    int? PortfolioCompanyCount,
    string? InvestmentThesis,
    bool IsAcceptingPitches) : IRequest<InvestorProfileDto>;
