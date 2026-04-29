using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MatchingApi.Helpers;
using Pgvector;

namespace MatchingApi.Models;

public class Startup
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? Twitter { get; set; }

    [MaxLength(500)]
    public string? Instagram { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Alive";

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? YearFounded { get; set; }

    [MaxLength(200)]
    public string? HQ { get; set; }

    [MaxLength(500)]
    public string? Founders { get; set; }

    /// <summary>Comma-separated tags: "Fintech, AI, SaaS"</summary>
    [MaxLength(1000)]
    public string? Tags { get; set; }

    /// <summary>Comma-separated: "B2B, B2C"</summary>
    [MaxLength(100)]
    public string? BusinessModel { get; set; }

    [MaxLength(500)]
    public string? RevenueModel { get; set; }

    [MaxLength(50)]
    public string? RevenueState { get; set; }

    [MaxLength(50)]
    public string? TotalFunding { get; set; }

    [MaxLength(50)]
    public string? Stage { get; set; }

    [MaxLength(500)]
    public string? WebsiteEmail { get; set; }

    [MaxLength(4000)]
    public string? WebsiteDescription { get; set; }

    // ── Vector embedding ──
    public Vector? Embedding { get; set; }

    [MaxLength(32)]
    public string? EmbeddingHash { get; set; }  // MD5(text) — stale detection

    // ── Computed / cached fields ──
    [NotMapped]
    public List<string> ParsedTags => ModelHelpers.ParseCsv(Tags);

    [NotMapped]
    public List<string> ParsedBusinessModels => ModelHelpers.ParseCsv(BusinessModel);

    [NotMapped]
    public (string City, string Country) ParsedHQ => ParseHqField(HQ);

    private static (string City, string Country) ParseHqField(string? hq)
    {
        if (string.IsNullOrWhiteSpace(hq)) return ("", "");
        var parts = hq.Split('/', StringSplitOptions.TrimEntries);
        return parts.Length >= 2
            ? (parts[0].Trim(), parts[1].Trim())
            : (parts[0].Trim(), "");
    }
}
