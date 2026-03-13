using System.ComponentModel.DataAnnotations;

namespace MatchingApi.Models;

public class MatchResult
{
    [Key]
    public long Id { get; set; }

    [Required, MaxLength(50)]
    public string InvestorId { get; set; } = string.Empty;

    public int StartupId { get; set; }

    /// <summary>rule-based | ai-powered</summary>
    [MaxLength(20)]
    public string MatchingMode { get; set; } = "rule-based";

    public double TotalScore { get; set; }
    public double SectorScore { get; set; }
    public double GeoScore { get; set; }
    public double ModelScore { get; set; }
    public double StageScore { get; set; }
    public double FundingBonus { get; set; }
    public double SemanticScore { get; set; }
    public double LlmBonus { get; set; }

    [MaxLength(2000)]
    public string? AiReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Startup? Startup { get; set; }
    public Investor? Investor { get; set; }
}
