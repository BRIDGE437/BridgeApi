namespace MatchingApi.DTOs;

// ── Request DTOs ──

public record MatchRequest(
    string InvestorId,
    int TopN = 10
);

public record InvestorCreateDto(
    string Name,
    string Type,
    string PreferredSectors,
    string PreferredBusinessModel,
    string PreferredRegions,
    string? PreferredCities,
    string InvestmentStage,
    long TicketSizeMin,
    long TicketSizeMax,
    string? PreferredRevenueState,
    string? Portfolio,
    string? Description,
    string? Website,
    string? ContactEmail,
    string? LinkedIn
);

public record InvestorUpdateDto(
    string? Name,
    string? Type,
    string? PreferredSectors,
    string? PreferredBusinessModel,
    string? PreferredRegions,
    string? PreferredCities,
    string? InvestmentStage,
    long? TicketSizeMin,
    long? TicketSizeMax,
    string? PreferredRevenueState,
    string? Portfolio,
    string? Description,
    string? Website,
    string? ContactEmail,
    string? LinkedIn,
    bool? Active
);

// ── Response DTOs ──

public record MatchResultDto(
    int Rank,
    int StartupId,
    string StartupName,
    double Score,
    ScoreBreakdownDto Breakdown,
    string? AiReason,
    List<string> Tags,
    string? HQ,
    string? BusinessModel,
    string? Description,
    string? RevenueState,
    string? Website
);

public record ScoreBreakdownDto(
    double SectorScore,
    double GeoScore,
    double ModelScore,
    double StageScore,
    double FundingBonus,
    double SemanticScore,
    double LlmBonus
);

public record MatchResponseDto(
    string InvestorId,
    string InvestorName,
    string MatchingMode,
    int TotalCandidates,
    List<MatchResultDto> Results,
    MatchMetadataDto Metadata
);

public record MatchMetadataDto(
    long ProcessingTimeMs,
    string? EmbeddingModel,
    bool LlmUsed
);

public record StartupDto(
    int Id,
    string Name,
    string? Website,
    string? Status,
    string? Description,
    int? YearFounded,
    string? HQ,
    string? Founders,
    List<string> Tags,
    List<string> BusinessModels,
    string? RevenueModel,
    string? RevenueState,
    string? TotalFunding,
    string? Stage
);

public record ImportResultDto(
    int TotalRows,
    int Imported,
    int Skipped,
    List<string> Errors
);
