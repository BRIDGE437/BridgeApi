using BridgeApi.Shared.Entities;
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

        var investor = await _db.InvestorProfiles.FindAsync(investorId)
            ?? throw new KeyNotFoundException($"InvestorProfile not found: {investorId}");

        var startups = await GetCandidateStartupsAsync(investor);

        _logger.LogInformation("Loaded {Count} alive startups for matching", startups.Count);

        // ── Cache investor parsed fields once — avoid re-parsing in the hot loop ──
        var investorRegions = MatchingApi.Helpers.ModelHelpers.ParseCsv(investor.PreferredRegions);
        var investorCities  = new List<string>(); // Cities removed from InvestorProfile
        var investorSectors = MatchingApi.Helpers.ModelHelpers.ParseCsv(investor.PreferredSectors);
        var investorModels  = MatchingApi.Helpers.ModelHelpers.ParseCsv(investor.PreferredBusinessModel);

        bool hasModelPref = investorModels.Count > 0;

        // With model pref:    Sector 40 + Geo 35 + Model 25 = 100
        // Without model pref: Sector 55 + Geo 45             = 100
        double maxSector = hasModelPref ? 40.0 : 55.0;
        double maxGeo    = hasModelPref ? 35.0 : 45.0;
        double maxModel  = hasModelPref ? 25.0 : 0.0;

        var scored = new List<(StartupProfile StartupProfile, double Total, double Sector, double Geo, double Model)>();

        foreach (var startup in startups)
        {
            var (city, country) = MatchingApi.Helpers.ModelHelpers.ParseHq(startup.HQ);

            // ── Hard filter: region check ──
            var region = RegionMapper.GetRegion(country);
            var hasRegionOverlap =
                investorRegions.Any(r => r.Equals(country, StringComparison.OrdinalIgnoreCase)) ||
                investorRegions.Any(r => r.Equals(region, StringComparison.OrdinalIgnoreCase));

            if (!hasRegionOverlap && investorRegions.Count > 0)
                continue;

            // ── 1. Sector Score (similarity-aware) ──
            double sectorScore = SectorSimilarity.CalculateSectorScore(
                MatchingApi.Helpers.ModelHelpers.ParseCsv(startup.Tags), investorSectors, maxSector);

            // ── 2. Geographic Score (proximity-aware) ──
            double geoScore = RegionMapper.CalculateGeoScore(
                city, country, investorCities, investorRegions, maxGeo);

            // ── 3. Business Model Score (skip if investor has no preference) ──
            double modelScore = 0;
            if (hasModelPref)
            {
                modelScore = CalculateBusinessModelScore(
                    MatchingApi.Helpers.ModelHelpers.ParseCsv(startup.BusinessModel), investorModels, maxModel);
            }

            double total = sectorScore + geoScore + modelScore;

            scored.Add((startup, total, sectorScore, geoScore, modelScore));
        }

        var topResults = scored
            .OrderByDescending(s => s.Total)
            .Take(topN)
            .Select((item, index) => new MatchResultDto(
                Rank: index + 1,
                StartupId: item.StartupProfile.UserId,
                StartupName: item.StartupProfile.CompanyName ?? "",
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
                Tags: MatchingApi.Helpers.ModelHelpers.ParseCsv(item.StartupProfile.Tags),
                HQ: item.StartupProfile.HQ,
                BusinessModel: item.StartupProfile.BusinessModel,
                Description: item.StartupProfile.Description,
                RevenueState: item.StartupProfile.RevenueState,
                Website: item.StartupProfile.WebsiteUrl
            ))
            .ToList();

        await PersistResultsAsync(investorId, "rule-based", topResults);
        sw.Stop();

        return new MatchResponseDto(
            InvestorId: investorId,
            InvestorName: investor.CompanyName,
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
    // STARTUP-TO-STARTUP MATCHING
    // ══════════════════════════════════════════════
    
    /// <summary>
    /// Calculates B2B synergy between a source startup and a list of participant startups.
    /// Used for StartupProfile-to-StartupProfile networking events.
    /// </summary>
    public async Task<MatchResponseDto> MatchStartupsAsync(string sourceStartupId, List<StartupProfile> participants, int topN = 10)
    {
        var sw = Stopwatch.StartNew();

        var sourceStartup = await _db.StartupProfiles.FindAsync(sourceStartupId)
            ?? throw new KeyNotFoundException($"Source startup not found: {sourceStartupId}");

        var sourceTags = MatchingApi.Helpers.ModelHelpers.ParseCsv(sourceStartup.Tags);
        var (sourceCity, sourceCountry) = MatchingApi.Helpers.ModelHelpers.ParseHq(sourceStartup.HQ);
        var sourceStage = sourceStartup.Stage ?? "";

        // B2B Synergy weights
        double maxSector = 40.0;
        double maxGeo = 30.0;
        double maxStage = 30.0;

        var scored = new List<(StartupProfile StartupProfile, double Total, double Sector, double Geo, double Stage, double Model)>();

        foreach (var target in participants)
        {
            if (target.UserId == sourceStartupId) continue; // Skip self

            var (targetCity, targetCountry) = MatchingApi.Helpers.ModelHelpers.ParseHq(target.HQ);

            // 1. Sector/Tags Synergy (Pass source tags as if they are investor preferences)
            double sectorScore = SectorSimilarity.CalculateSectorScore(
                MatchingApi.Helpers.ModelHelpers.ParseCsv(target.Tags), sourceTags, maxSector);

            // 2. Geo-Proximity
            double geoScore = 0;
            if (sourceCountry.Equals(targetCountry, StringComparison.OrdinalIgnoreCase))
            {
                geoScore = maxGeo * 0.8; // Same country
                if (sourceCity.Equals(targetCity, StringComparison.OrdinalIgnoreCase))
                {
                    geoScore = maxGeo; // Same city
                }
            }

            // 4. Business Model Synergy (Bonus points for complementary models)
            double modelScore = CalculateBusinessModelScore(
                MatchingApi.Helpers.ModelHelpers.ParseCsv(target.BusinessModel), 
                MatchingApi.Helpers.ModelHelpers.ParseCsv(sourceStartup.BusinessModel), 10.0);

            // Assuming stageScore was supposed to be 0 for now as it's not defined in the snippet above
            double stageScore = 0; 

            double total = sectorScore + geoScore + stageScore + modelScore;

            scored.Add((target, total, sectorScore, geoScore, stageScore, modelScore));
        }

        var topResults = scored
            .OrderByDescending(s => s.Total)
            .Take(topN)
            .Select((item, index) => new MatchResultDto(
                Rank: index + 1,
                StartupId: item.StartupProfile.UserId,
                StartupName: item.StartupProfile.CompanyName ?? "",
                Score: Math.Round(item.Total, 1),
                Breakdown: new ScoreBreakdownDto(
                    SectorScore: Math.Round(item.Sector, 1),
                    GeoScore: Math.Round(item.Geo, 1),
                    ModelScore: Math.Round(item.Model, 1),
                    StageScore: Math.Round(item.Stage, 1),
                    FundingBonus: 0,
                    SemanticScore: 0,
                    LlmBonus: 0
                ),
                AiReason: null,
                Tags: MatchingApi.Helpers.ModelHelpers.ParseCsv(item.StartupProfile.Tags),
                HQ: item.StartupProfile.HQ,
                BusinessModel: item.StartupProfile.BusinessModel,
                Description: item.StartupProfile.Description,
                RevenueState: item.StartupProfile.RevenueState,
                Website: item.StartupProfile.WebsiteUrl
            ))
            .ToList();

        sw.Stop();

        return new MatchResponseDto(
            InvestorId: sourceStartupId.ToString(), // Temporary mapping for the DTO
            InvestorName: sourceStartup.CompanyName ?? "",
            MatchingMode: "rule-based-startup",
            TotalCandidates: participants.Count,
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
    private async Task<List<StartupProfile>> GetCandidateStartupsAsync(InvestorProfile investor)
    {
        var terms = MatchingApi.Helpers.ModelHelpers.ParseCsv(investor.PreferredRegions)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .ToList();

        if (!terms.Any())
            return await _db.StartupProfiles.ToListAsync();

        // Build: "HQ" ILIKE {0} OR "HQ" ILIKE {1} ...
        var conditions = terms.Select((_, i) => $@"""HQ"" ILIKE {{{i}}}");
        var sql = $@"SELECT * FROM ""StartupProfiles"" WHERE ({string.Join(" OR ", conditions)})";
        var parameters = terms.Select(t => (object)$"%{t}%").ToArray();

        return await _db.StartupProfiles.FromSqlRaw(sql, parameters).ToListAsync();
    }

    // ── Persistence — upsert by (InvestorId, StartupId, MatchingMode) ──
    private async Task PersistResultsAsync(string investorId, string mode, List<MatchResultDto> results)
    {
        var startupIds = results.Select(r => r.StartupId).ToList();
        var existing = await _db.MatchResults
            .Where(m => m.InvestorId == investorId && m.MatchingMode == mode && startupIds.Contains(m.StartupId))
            .ToDictionaryAsync(m => m.StartupId);

        foreach (var r in results)
        {
            if (existing.TryGetValue(r.StartupId, out var row))
            {
                row.TotalScore = r.Score;
                row.SectorScore = r.Breakdown.SectorScore;
                row.GeoScore = r.Breakdown.GeoScore;
                row.ModelScore = r.Breakdown.ModelScore;
                row.SemanticScore = r.Breakdown.SemanticScore;
                row.LlmBonus = r.Breakdown.LlmBonus;
                row.AiReason = r.AiReason;
                row.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.MatchResults.Add(new MatchResult
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
            }
        }

        await _db.SaveChangesAsync();
    }
}
