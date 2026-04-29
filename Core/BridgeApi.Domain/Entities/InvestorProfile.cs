using BridgeApi.Domain.Enums;

namespace BridgeApi.Domain.Entities;

public class InvestorProfile : BaseEntity
{
    public Guid UserProfileId { get; set; }

    public string? FirmName { get; set; }
    public string? FirmWebsite { get; set; }

    public decimal CheckSizeMinUsd { get; set; }
    public decimal CheckSizeMaxUsd { get; set; }

    public InvestmentStage[] PreferredStages { get; set; } = Array.Empty<InvestmentStage>();
    public Sector[] PreferredSectors { get; set; } = Array.Empty<Sector>();
    public string[] PreferredGeographies { get; set; } = Array.Empty<string>();

    public int? PortfolioCompanyCount { get; set; }
    public string? InvestmentThesis { get; set; }
    public bool IsAcceptingPitches { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}
