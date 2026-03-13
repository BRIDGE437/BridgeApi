using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Helpers;
using MatchingApi.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

namespace MatchingApi.Services;

public class RuleBasedMatchingService
{
    private readonly AppDbContext _db;
    private readonly ILogger<RuleBasedMatchingService> _logger;

    public RuleBasedMatchingService(AppDbContext db, ILogger<RuleBasedMatchingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MatchResponseDto> MatchAsync(string investorId, int topN = 10)
    {
        var sw = Stopwatch.StartNew();

        var investor = await _db.Investors.FindAsync(investorId)
            ?? throw new KeyNotFoundException($"Investor not found: {investorId}");

        var startups = await GetCandidateStartupsAsync(investor);

        _logger.LogInformation("Loaded {Count} alive startups for matching", startups.Count);

        // ── Determine weights based on whether investor has a model preference ──
        bool hasModelPref = investor.ParsedBusinessModels.Count > 0;

        // With model pref:    Sector 40 + Geo 35 + Model 25 = 100
        // Without model pref: Sector 55 + Geo 45             = 100
        double maxSector = hasModelPref ? 40.0 : 55.0;
        double maxGeo    = hasModelPref ? 35.0 : 45.0;
        double maxModel  = hasModelPref ? 25.0 : 0.0;

        var scored = new List<(Startup Startup, double Total, double Sector, double Geo, double Model)>();

        foreach (var startup in startups)
        {
            var (city, country) = startup.ParsedHQ;

            // ── Hard filter: region check ──
            var region = RegionMapper.GetRegion(country);
            var investorRegions = investor.ParsedRegions;
            var hasRegionOverlap =
                investorRegions.Any(r => r.Equals(country, StringComparison.OrdinalIgnoreCase)) ||
                investorRegions.Any(r => r.Equals(region, StringComparison.OrdinalIgnoreCase)) ||
                investor.ParsedCities.Any(c => c.Equals(city, StringComparison.OrdinalIgnoreCase)) ||
                HasNearbyCity(city, investor.ParsedCities);

            if (!hasRegionOverlap && investorRegions.Count > 0)
                continue;

            // ── 1. Sector Score (similarity-aware) ──
            double sectorScore = SectorSimilarity.CalculateSectorScore(
                startup.ParsedTags, investor.ParsedSectors, maxSector);

            // ── 2. Geographic Score (proximity-aware) ──
            double geoScore = RegionMapper.CalculateGeoScore(
                city, country, investor.ParsedCities, investor.ParsedRegions, maxGeo);

            // ── 3. Business Model Score (skip if investor has no preference) ──
            double modelScore = 0;
            if (hasModelPref)
            {
                modelScore = CalculateBusinessModelScore(
                    startup.ParsedBusinessModels, investor.ParsedBusinessModels, maxModel);
            }

            double total = sectorScore + geoScore + modelScore;

            scored.Add((startup, total, sectorScore, geoScore, modelScore));
        }

        var topResults = scored
            .OrderByDescending(s => s.Total)
            .Take(topN)
            .Select((item, index) => new MatchResultDto(
                Rank: index + 1,
                StartupId: item.Startup.Id,
                StartupName: item.Startup.Name,
                Score: Math.Round(item.Total, 1),
                Breakdown: new ScoreBreakdownDto(
                    SectorScore: Math.Round(item.Sector, 1),
                    GeoScore: Math.Round(item.Geo, 1),
                    ModelScore: Math.Round(item.Model, 1),
                    StageScore: 0,
                    FundingBonus: 0,
                    SemanticScore: 0,
                    LlmBonus: 0
                ),
                AiReason: null,
                Tags: item.Startup.ParsedTags,
                HQ: item.Startup.HQ,
                BusinessModel: item.Startup.BusinessModel,
                Description: item.Startup.Description,
                RevenueState: item.Startup.RevenueState,
                Website: item.Startup.Website
            ))
            .ToList();

        await PersistResultsAsync(investorId, "rule-based", topResults);
        sw.Stop();

        return new MatchResponseDto(
            InvestorId: investorId,
            InvestorName: investor.Name,
            MatchingMode: "rule-based",
            TotalCandidates: scored.Count,
            Results: topResults,
            Metadata: new MatchMetadataDto(
                ProcessingTimeMs: sw.ElapsedMilliseconds,
                EmbeddingModel: null,
                LlmUsed: false
            )
        );
    }

    // ══════════════════════════════════════════════
    // SCORING FUNCTIONS
    // ══════════════════════════════════════════════

    /// <summary>
    /// Business model matching.
    /// Full overlap = maxScore, Partial = maxScore × 0.5, None = 0.
    /// </summary>
    public static double CalculateBusinessModelScore(
        List<string> startupModels, List<string> investorModels, double maxScore)
    {
        if (startupModels.Count == 0 || investorModels.Count == 0) return 0;

        var overlap = startupModels
            .Count(sm => investorModels.Any(im => im.Equals(sm, StringComparison.OrdinalIgnoreCase)));

        if (overlap == startupModels.Count) return maxScore;       // Full match
        if (overlap > 0) return Math.Round(maxScore * 0.5, 1);     // Partial match
        return 0.0;
    }

    /// <summary>
    /// Checks if any startup city is near an investor's preferred city.
    /// Used in hard filter to not exclude nearby cities.
    /// </summary>
    private static bool HasNearbyCity(string city, List<string> preferredCities)
    {
        if (string.IsNullOrWhiteSpace(city)) return false;
        return preferredCities.Any(pc => RegionMapper.GetCityProximity(city, pc) > 0);
    }

    // ── DB-level pre-filter ──

    /// <summary>
    /// Fetches alive startups, filtering HQ in the database when the investor
    /// has explicit region/city preferences. Reduces in-memory set before C# scoring.
    /// </summary>
    private async Task<List<Startup>> GetCandidateStartupsAsync(Investor investor)
    {
        var terms = investor.ParsedRegions
            .Concat(investor.ParsedCities)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .ToList();

        if (!terms.Any())
            return await _db.Startups.Where(s => s.Status == "Alive").ToListAsync();

        // Build: "HQ" ILIKE {0} OR "HQ" ILIKE {1} ...
        var conditions = terms.Select((_, i) => $@"""HQ"" ILIKE {{{i}}}");
        var sql = $@"SELECT * FROM ""Startups"" WHERE ""Status"" = 'Alive' AND ({string.Join(" OR ", conditions)})";
        var parameters = terms.Select(t => (object)$"%{t}%").ToArray();

        return await _db.Startups.FromSqlRaw(sql, parameters).ToListAsync();
    }

    // ── Persistence ──
    private async Task PersistResultsAsync(string investorId, string mode, List<MatchResultDto> results)
    {
        var entities = results.Select(r => new MatchResult
        {
            InvestorId = investorId,
            StartupId = r.StartupId,
            MatchingMode = mode,
            TotalScore = r.Score,
            SectorScore = r.Breakdown.SectorScore,
            GeoScore = r.Breakdown.GeoScore,
            ModelScore = r.Breakdown.ModelScore,
            StageScore = 0,
            FundingBonus = 0,
            SemanticScore = r.Breakdown.SemanticScore,
            LlmBonus = r.Breakdown.LlmBonus,
            AiReason = r.AiReason,
        });

        _db.MatchResults.AddRange(entities);
        await _db.SaveChangesAsync();
    }
}
