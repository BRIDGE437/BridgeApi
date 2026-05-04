using System.ComponentModel.DataAnnotations;

namespace BridgeApi.Shared.Entities;

public class InvestorProfile
{
    [Key]
    public string UserId { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Type { get; set; } = "angel"; // angel, vc, etc.

    [Required, MaxLength(1000)]
    public string PreferredSectors { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string PreferredBusinessModel { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string PreferredRegions { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string InvestmentStage { get; set; } = string.Empty;

    public long TicketSizeMin { get; set; }
    public long TicketSizeMax { get; set; }

    [MaxLength(200)]
    public string? PreferredRevenueState { get; set; }

    [MaxLength(2000)]
    public string? Portfolio { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? CompanyName { get; set; } // For institutional investors

    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "vector(384)")]
    public Pgvector.Vector? Embedding { get; set; }

    [MaxLength(32)]
    public string? EmbeddingHash { get; set; } // MD5 hash for stale detection
}
