using System.ComponentModel.DataAnnotations;

namespace MatchingApi.Models;

/// <summary>
/// Caches LLM reranking results keyed by (investor text hash, startup id, startup text hash).
/// Prevents repeated OpenAI/Anthropic calls when neither investor profile nor startup text has changed.
/// </summary>
public class LlmScoreCache
{
    public long Id { get; set; }

    /// <summary>MD5 of the investor profile text at time of scoring.</summary>
    [Required, MaxLength(32)]
    public string InvestorTextHash { get; set; } = string.Empty;

    public int StartupId { get; set; }

    /// <summary>MD5 of the startup text at time of scoring.</summary>
    [Required, MaxLength(32)]
    public string StartupTextHash { get; set; } = string.Empty;

    /// <summary>LLM score 0–10.</summary>
    public double Score { get; set; }

    [MaxLength(2000)]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
