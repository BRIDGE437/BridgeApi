using System.ComponentModel.DataAnnotations;

namespace BridgeApi.Shared.Entities;

public class StartupProfile
{
    [Key]
    public string UserId { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Stage { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Tags { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string BusinessModel { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RevenueModel { get; set; }

    [MaxLength(100)]
    public string? RevenueState { get; set; }

    public long TotalFunding { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(5000)]
    public string? WebsiteDescription { get; set; }

    [MaxLength(500)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(200)]
    public string? CompanyName { get; set; }

    [MaxLength(200)]
    public string? HQ { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "vector(384)")]
    public Pgvector.Vector? Embedding { get; set; }

    [MaxLength(46)]
    public string? EmbeddingHash { get; set; } // MD5 hash for stale detection

    [MaxLength(2000)]
    public string? ContactEmails { get; set; }
    
    public bool NeedsManualReview { get; set; } = false; // Flag for scraped data that needs admin review
    
    [MaxLength(64)]
    public string? ExternalFingerprint { get; set; } // Unique fingerprint for deduplication
}
