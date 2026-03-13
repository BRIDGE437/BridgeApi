using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MatchingApi.Helpers;
using Pgvector;

namespace MatchingApi.Models;

public class Investor
{
    [Key]
    [MaxLength(50)]
    public string InvestorId { get; set; } = Guid.NewGuid().ToString("N")[..12];

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>angel | vc | corporate | accelerator | family_office</summary>
    [Required, MaxLength(50)]
    public string Type { get; set; } = "angel";

    /// <summary>Comma-separated: "Fintech, AI, SaaS"</summary>
    [Required, MaxLength(1000)]
    public string PreferredSectors { get; set; } = string.Empty;

    /// <summary>Comma-separated: "B2B, B2C"</summary>
    [Required, MaxLength(100)]
    public string PreferredBusinessModel { get; set; } = string.Empty;

    /// <summary>Comma-separated: "Turkey, Europe, USA"</summary>
    [Required, MaxLength(500)]
    public string PreferredRegions { get; set; } = string.Empty;

    /// <summary>Comma-separated: "Istanbul, London"</summary>
    [MaxLength(500)]
    public string? PreferredCities { get; set; }

    /// <summary>Comma-separated: "Pre-Seed, Seed, Series A"</summary>
    [Required, MaxLength(200)]
    public string InvestmentStage { get; set; } = string.Empty;

    public long TicketSizeMin { get; set; }
    public long TicketSizeMax { get; set; }

    /// <summary>Comma-separated: "Pre-Revenue, Post-Revenue"</summary>
    [MaxLength(200)]
    public string? PreferredRevenueState { get; set; }

    [MaxLength(2000)]
    public string? Portfolio { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(500)]
    public string? LinkedIn { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Vector embedding ──
    public Vector? Embedding { get; set; }

    [MaxLength(32)]
    public string? EmbeddingHash { get; set; }  // MD5(text) — stale detection

    // ── NotMapped computed ──
    [NotMapped]
    public List<string> ParsedSectors => ModelHelpers.ParseCsv(PreferredSectors);
    [NotMapped]
    public List<string> ParsedBusinessModels => ModelHelpers.ParseCsv(PreferredBusinessModel);
    [NotMapped]
    public List<string> ParsedRegions => ModelHelpers.ParseCsv(PreferredRegions);
    [NotMapped]
    public List<string> ParsedCities => ModelHelpers.ParseCsv(PreferredCities);
    [NotMapped]
    public List<string> ParsedStages => ModelHelpers.ParseCsv(InvestmentStage);
    [NotMapped]
    public List<string> ParsedRevenueStates => ModelHelpers.ParseCsv(PreferredRevenueState);
}
