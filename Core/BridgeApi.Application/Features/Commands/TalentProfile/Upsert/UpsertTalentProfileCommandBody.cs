using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Features.Commands.TalentProfile.Upsert;

public record UpsertTalentProfileCommandBody(
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
    Sector[] InterestedSectors);
