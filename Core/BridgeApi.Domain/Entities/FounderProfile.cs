using BridgeApi.Domain.Enums;

namespace BridgeApi.Domain.Entities;

public class FounderProfile : BaseEntity
{
    public Guid UserProfileId { get; set; }

    public string StartupName { get; set; } = null!;
    public string? StartupWebsite { get; set; }
    public StartupStage Stage { get; set; }
    public Sector PrimarySector { get; set; }
    public Sector[] SecondarySectors { get; set; } = Array.Empty<Sector>();

    public decimal? FundingNeedUsd { get; set; }
    public int? TeamSize { get; set; }
    public string? PitchDeckUrl { get; set; }
    public string? OneLiner { get; set; }
    public string? ProblemStatement { get; set; }
    public int? FoundedYear { get; set; }
    public string? Country { get; set; }
    public bool IsActivelyFundraising { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
