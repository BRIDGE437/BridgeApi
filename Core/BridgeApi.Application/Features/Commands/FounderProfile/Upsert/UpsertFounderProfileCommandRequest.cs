using BridgeApi.Application.Dtos.Profiles;
using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.FounderProfile.Upsert;

public record UpsertFounderProfileCommandRequest(
    string UserId,
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
    bool IsActivelyFundraising) : IRequest<FounderProfileDto>;
