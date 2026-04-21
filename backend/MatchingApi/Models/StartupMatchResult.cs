using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchingApi.Models;

/// <summary>
/// Represents a B2B matching result evaluating synergy between two startups.
/// </summary>
public class StartupMatchResult
{
    [Key]
    public long Id { get; set; }

    public int? EventId { get; set; }
    
    [ForeignKey(nameof(EventId))]
    public MatchEvent? Event { get; set; }

    // ── The startup that initiated the match (or the primary subject) ──
    public int SourceStartupId { get; set; }
    [ForeignKey(nameof(SourceStartupId))]
    public Startup? SourceStartup { get; set; }

    // ── The candidate startup being evaluated ──
    public int TargetStartupId { get; set; }
    [ForeignKey(nameof(TargetStartupId))]
    public Startup? TargetStartup { get; set; }

    public double TotalScore { get; set; }
    
    /// <summary>Points awarded for sector and tags synergy (e.g. Fintech + AI)</summary>
    public double SectorScore { get; set; }
    
    /// <summary>Points awarded for geographic proximity or HQ country matches</summary>
    public double GeoScore { get; set; }
    
    /// <summary>Points awarded for investment stage alignment</summary>
    public double StageScore { get; set; }
    
    /// <summary>Cosine similarity score from vector embeddings (Python AI)</summary>
    public double SemanticScore { get; set; }
    
    /// <summary>Bonus points from Large Language Model (Gemini) analysis</summary>
    public double LlmBonus { get; set; }

    [MaxLength(2000)]
    public string? AiReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
