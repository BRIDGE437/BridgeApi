using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.FounderProfile.Upsert;

public record UpsertFounderProfileCommandBody(
    string StartupName,
    string? StartupWebsite,
    StartupStage Stage,
    Sector PrimarySector,
    Sector[] SecondarySectors,
    decimal? FundingNeedUsd,
    int? TeamSize,
    string? PitchDeckUrl,
    string? OneLiner,
    string? ProblemStatement,
    int? FoundedYear,
    string? Country,
    bool IsActivelyFundraising);
