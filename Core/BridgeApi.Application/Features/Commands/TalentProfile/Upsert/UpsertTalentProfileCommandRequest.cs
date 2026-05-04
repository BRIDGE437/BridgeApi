using BridgeApi.Application.Dtos.Profiles;
using BridgeApi.Domain.Enums;
using MediatR;

namespace BridgeApi.Application.Features.Commands.TalentProfile.Upsert;

public record UpsertTalentProfileCommandRequest(
    string UserId,
    string Headline,
    string[] Skills,
    EmploymentType[] LookingFor,
    WorkPreference WorkPreference,
    int YearsOfExperience,
    decimal? ExpectedSalaryMonthlyUsd,
    bool OpenToWork,
    DateTime? AvailableFrom,
    string? CurrentRole,
    string? CurrentCompany,
    Sector[] InterestedSectors) : IRequest<TalentProfileDto>;
