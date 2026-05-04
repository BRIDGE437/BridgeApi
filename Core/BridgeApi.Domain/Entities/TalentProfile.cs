using BridgeApi.Domain.Enums;

namespace BridgeApi.Domain.Entities;

public class TalentProfile : BaseEntity
{
    public Guid UserProfileId { get; set; }

    public string Headline { get; set; } = null!;
    public string[] Skills { get; set; } = Array.Empty<string>();
    public EmploymentType[] LookingFor { get; set; } = Array.Empty<EmploymentType>();
    public WorkPreference WorkPreference { get; set; }

    public int YearsOfExperience { get; set; }
    public decimal? ExpectedSalaryMonthlyUsd { get; set; }
    public bool OpenToWork { get; set; }
    public DateTime? AvailableFrom { get; set; }

    public string? CurrentRole { get; set; }
    public string? CurrentCompany { get; set; }
    public Sector[] InterestedSectors { get; set; } = Array.Empty<Sector>();

    public UserProfile UserProfile { get; set; } = null!;
}
