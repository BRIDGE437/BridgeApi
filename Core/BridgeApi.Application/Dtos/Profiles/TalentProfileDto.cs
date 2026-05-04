using BridgeApi.Domain.Enums;

namespace BridgeApi.Application.Dtos.Profiles;

public record TalentProfileDto(
    Guid Id,
    Guid UserProfileId,
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
