using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace MatchingApi.Services;

public class AiMatchingService
{
    private readonly AppDbContext _db;
    private readonly RuleBasedMatchingService _ruleEngine;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiMatchingService> _logger;
    private readonly IConfiguration _config;

    public AiMatchingService(
        AppDbContext db,
        RuleBasedMatchingService ruleEngine,
        IHttpClientFactory httpClientFactory,
        ILogger<AiMatchingService> logger,
        IConfiguration config)
    {
        _db = db;
        _ruleEngine = ruleEngine;
        _httpClient = httpClientFactory.CreateClient("AiService");
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Hybrid matching: Rule-based pre-filter → Semantic similarity → Optional LLM rerank
    /// </summary>
    public async Task<MatchResponseDto> MatchAsync(string investorId, int topN = 10)
    {
        var sw = Stopwatch.StartNew();

        var investor = await _db.Investors.FindAsync(investorId)
            ?? throw new KeyNotFoundException($"Investor not found: {investorId}");

        // ── Phase 1: Rule-based pre-filtering (top 50) ──
        var ruleResults = await _ruleEngine.MatchAsync(investorId, topN: 50);
        _logger.LogInformation("Rule-based pre-filter returned {Count} candidates", ruleResults.Results.Count);

        // ── Phase 2: Semantic similarity via Python AI service ──
        var embeddingModel = "all-MiniLM-L6-v2";
        var llmUsed = false;

        List<SemanticResult>? semanticScores = null;
        try
        {
            semanticScores = await GetSemanticScoresAsync(investor, ruleResults.Results);
            _logger.LogInformation("Semantic scoring completed for {Count} candidates", semanticScores?.Count ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI service unavailable, falling back to rule-based only");
        }

        // ── Phase 3: Combine scores ──
        var semanticById = semanticScores?.ToDictionary(s => s.StartupId) ?? new();
        var hybridResults = new List<MatchResultDto>();

        foreach (var rule in ruleResults.Results)
        {
            double ruleScore = rule.Score * 0.6; // 60% weight
            double semanticScore = 0;
            double llmBonus = 0;
            string? aiReason = null;

            if (semanticById.TryGetValue(rule.StartupId, out var sem))
            {
                semanticScore = sem.SimilarityScore * 30; // 30% weight
                aiReason = sem.Reason;
                llmBonus = sem.LlmScore; // 10% weight (if available)
                if (llmBonus > 0) llmUsed = true;
            }

            double hybridTotal = Math.Min(100, ruleScore + semanticScore + llmBonus);

            hybridResults.Add(rule with
            {
                Score = Math.Round(hybridTotal, 1),
                Breakdown = rule.Breakdown with
                {
                    SectorScore = Math.Round(rule.Breakdown.SectorScore * 0.6, 1),
                    GeoScore = Math.Round(rule.Breakdown.GeoScore * 0.6, 1),
                    ModelScore = Math.Round(rule.Breakdown.ModelScore * 0.6, 1),
                    StageScore = Math.Round(rule.Breakdown.StageScore * 0.6, 1),
                    FundingBonus = Math.Round(rule.Breakdown.FundingBonus * 0.6, 1),
                    SemanticScore = Math.Round(semanticScore, 1),
                    LlmBonus = Math.Round(llmBonus, 1),
                },
                AiReason = aiReason,
            });
        }

        // Re-sort by hybrid score and take top N
        var finalResults = hybridResults
            .OrderByDescending(r => r.Score)
            .Take(topN)
            .Select((r, idx) => r with { Rank = idx + 1 })
            .ToList();

        // Persist
        await PersistResultsAsync(investorId, finalResults);

        sw.Stop();

        return new MatchResponseDto(
            InvestorId: investorId,
            InvestorName: investor.Name,
            MatchingMode: "ai-powered",
            TotalCandidates: ruleResults.TotalCandidates,
            Results: finalResults,
            Metadata: new MatchMetadataDto(
                ProcessingTimeMs: sw.ElapsedMilliseconds,
                EmbeddingModel: embeddingModel,
                LlmUsed: llmUsed
            )
        );
    }

    /// <summary>
    /// Event-based hybrid matching. Constrains candidates to event participants.
    /// </summary>
    public async Task<MatchResponseDto> MatchEventAsync(string investorId, int eventId, int topN, List<int> startupIds)
    {
        var sw = Stopwatch.StartNew();

        var investor = await _db.Investors.FindAsync(investorId)
            ?? throw new KeyNotFoundException($"Investor not found: {investorId}");

        // Fetch rule results and filter to only include participating startups
        var ruleResults = await _ruleEngine.MatchAsync(investorId, topN: Math.Max(100, startupIds.Count));
        var candidateResults = ruleResults.Results
            .Where(r => startupIds.Contains(r.StartupId))
            .ToList();

        _logger.LogInformation("Event restricted candidates to {Count} startups", candidateResults.Count);

        var embeddingModel = "all-MiniLM-L6-v2";
        var llmUsed = false;

        List<SemanticResult>? semanticScores = null;
        try
        {
            if (candidateResults.Any())
                semanticScores = await GetSemanticScoresAsync(investor, candidateResults);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI service unavailable, falling back to rule-based only");
        }

        var semanticById = semanticScores?.ToDictionary(s => s.StartupId) ?? new();
        var hybridResults = new List<MatchResultDto>();

        foreach (var rule in candidateResults)
        {
            double ruleScore = rule.Score * 0.6;
            double semanticScore = 0;
            double llmBonus = 0;
            string? aiReason = null;

            if (semanticById.TryGetValue(rule.StartupId, out var sem))
            {
                semanticScore = sem.SimilarityScore * 30;
                aiReason = sem.Reason;
                llmBonus = sem.LlmScore;
                if (llmBonus > 0) llmUsed = true;
            }

            double hybridTotal = Math.Min(100, ruleScore + semanticScore + llmBonus);

            hybridResults.Add(rule with
            {
                Score = Math.Round(hybridTotal, 1),
                Breakdown = rule.Breakdown with
                {
                    SectorScore = Math.Round(rule.Breakdown.SectorScore * 0.6, 1),
                    GeoScore = Math.Round(rule.Breakdown.GeoScore * 0.6, 1),
                    ModelScore = Math.Round(rule.Breakdown.ModelScore * 0.6, 1),
                    StageScore = Math.Round(rule.Breakdown.StageScore * 0.6, 1),
                    FundingBonus = Math.Round(rule.Breakdown.FundingBonus * 0.6, 1),
                    SemanticScore = Math.Round(semanticScore, 1),
                    LlmBonus = Math.Round(llmBonus, 1),
                },
                AiReason = aiReason,
            });
        }

        var finalResults = hybridResults
            .OrderByDescending(r => r.Score)
            .Take(topN)
            .Select((r, idx) => r with { Rank = idx + 1 })
            .ToList();

        await PersistResultsAsync(investorId, finalResults, eventId);

        sw.Stop();

        return new MatchResponseDto(
            InvestorId: investorId,
            InvestorName: investor.Name,
            MatchingMode: "ai-powered",
            TotalCandidates: candidateResults.Count,
            Results: finalResults,
            Metadata: new MatchMetadataDto(
                ProcessingTimeMs: sw.ElapsedMilliseconds,
                EmbeddingModel: embeddingModel,
                LlmUsed: llmUsed
            )
        );
    }

    /// <summary>
    /// Event-based hybrid matching for Startup-to-Startup (B2B Networking).
    /// </summary>
    public async Task<MatchResponseDto> MatchStartupToStartupsAsync(int sourceStartupId, int eventId, int topN, List<Startup> participants)
    {
        var sw = Stopwatch.StartNew();

        var sourceStartup = await _db.Startups.FindAsync(sourceStartupId)
            ?? throw new KeyNotFoundException($"Source startup not found: {sourceStartupId}");

        // Fetch rule results
        var ruleResults = await _ruleEngine.MatchStartupsAsync(sourceStartupId, participants, topN: Math.Max(100, participants.Count));
        var candidateResults = ruleResults.Results;

        _logger.LogInformation("Startup-to-startup candidates: {Count}", candidateResults.Count);

        var embeddingModel = "all-MiniLM-L6-v2";
        var llmUsed = false;

        List<SemanticResult>? semanticScores = null;
        try
        {
            if (candidateResults.Any())
            {
                var request = new
                {
                    source_startup_id = sourceStartupId,
                    target_startup_ids = candidateResults.Select(c => c.StartupId).ToList(),
                    use_llm = false, 
                    mode = "startup_startup"
                };

                var response = await _httpClient.PostAsJsonAsync("/api/v1/semantic-match/startup-startup", request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<SemanticMatchResponse>(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        PropertyNameCaseInsensitive = true,
                    });
                
                if (result != null)
                {
                    semanticScores = result.Results;
                    embeddingModel = result.Model;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI service unavailable for B2B match, falling back to rule-based only");
        }

        var semanticById = semanticScores?.ToDictionary(s => s.StartupId) ?? new();
        var hybridResults = new List<MatchResultDto>();

        foreach (var rule in candidateResults)
        {
            double ruleScore = rule.Score * 0.6;
            double semanticScore = 0;
            double llmBonus = 0;
            string? aiReason = null;

            if (semanticById.TryGetValue(rule.StartupId, out var sem))
            {
                semanticScore = sem.SimilarityScore * 40; // B2B places high emphasis on semantic synergy
                aiReason = sem.Reason;
                llmBonus = sem.LlmScore;
                if (llmBonus > 0) llmUsed = true;
            }

            double hybridTotal = Math.Min(100, ruleScore + semanticScore + llmBonus);

            hybridResults.Add(rule with
            {
                Score = Math.Round(hybridTotal, 1),
                Breakdown = rule.Breakdown with
                {
                    SectorScore = Math.Round(rule.Breakdown.SectorScore * 0.6, 1),
                    GeoScore = Math.Round(rule.Breakdown.GeoScore * 0.6, 1),
                    StageScore = Math.Round(rule.Breakdown.StageScore * 0.6, 1),
                    SemanticScore = Math.Round(semanticScore, 1),
                    LlmBonus = Math.Round(llmBonus, 1),
                },
                AiReason = aiReason,
            });
        }

        var finalResults = hybridResults
            .OrderByDescending(r => r.Score)
            .Take(topN)
            .Select((r, idx) => r with { Rank = idx + 1 })
            .ToList();

        await PersistStartupResultsAsync(sourceStartupId, finalResults, eventId);

        sw.Stop();

        return new MatchResponseDto(
            InvestorId: sourceStartupId.ToString(), // Mapping to interface
            InvestorName: sourceStartup.Name,
            MatchingMode: "ai-powered-startup",
            TotalCandidates: candidateResults.Count,
            Results: finalResults,
            Metadata: new MatchMetadataDto(
                ProcessingTimeMs: sw.ElapsedMilliseconds,
                EmbeddingModel: embeddingModel,
                LlmUsed: llmUsed
            )
        );
    }

    // ══════════════════════════════════════════════
    // AI SERVICE COMMUNICATION
    // ══════════════════════════════════════════════

    public async Task IndexStartupsAsync(List<Startup> startups)
    {
        var request = new
        {
            startups = startups.Select(c => new
            {
                id = c.Id,
                text = $"{c.Name}. {c.Tags ?? ""}. " +
                       $"{c.Description ?? ""}. {c.BusinessModel ?? ""}. {c.HQ ?? ""}"
            }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("/api/v1/index-startups", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<List<SemanticResult>> GetSemanticScoresAsync(
        Investor investor, List<MatchResultDto> candidates)
    {
        var request = new
        {
            investor_text = BuildInvestorText(investor),
            startups = candidates.Select(c => new
            {
                id = c.StartupId,
                text = $"{c.StartupName}. {string.Join(", ", c.Tags)}. " +
                       $"{c.Description ?? ""}. {c.BusinessModel ?? ""}. {c.HQ ?? ""}"
            }).ToList(),
            use_llm = _config.GetValue<bool>("AiService:UseLlm", false)
        };

        var response = await _httpClient.PostAsJsonAsync("/api/v1/semantic-match", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SemanticMatchResponse>(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });

        return result?.Results ?? new List<SemanticResult>();
    }

    private static string BuildInvestorText(Investor investor)
    {
        var parts = new List<string>
        {
            investor.Name,
            $"Sectors: {investor.PreferredSectors}",
            $"Regions: {investor.PreferredRegions}",
            $"Stage: {investor.InvestmentStage}",
            $"Model: {investor.PreferredBusinessModel}",
        };
        if (!string.IsNullOrWhiteSpace(investor.Description))
            parts.Add(investor.Description);
        return string.Join(". ", parts);
    }

    private async Task PersistResultsAsync(string investorId, List<MatchResultDto> results, int? eventId = null)
    {
        var startupIds = results.Select(r => r.StartupId).ToList();
        var existing = await _db.MatchResults
            .Where(m => m.InvestorId == investorId && m.MatchingMode == "ai-powered" && startupIds.Contains(m.StartupId))
            .ToDictionaryAsync(m => m.StartupId);

        foreach (var r in results)
        {
            if (existing.TryGetValue(r.StartupId, out var row))
            {
                row.TotalScore = r.Score;
                row.SectorScore = r.Breakdown.SectorScore;
                row.GeoScore = r.Breakdown.GeoScore;
                row.ModelScore = r.Breakdown.ModelScore;
                row.StageScore = r.Breakdown.StageScore;
                row.FundingBonus = r.Breakdown.FundingBonus;
                row.SemanticScore = r.Breakdown.SemanticScore;
                row.LlmBonus = r.Breakdown.LlmBonus;
                row.AiReason = r.AiReason;
                row.EventId = eventId ?? row.EventId;
                row.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.MatchResults.Add(new MatchResult
                {
                    InvestorId = investorId,
                    StartupId = r.StartupId,
                    MatchingMode = "ai-powered",
                    TotalScore = r.Score,
                    SectorScore = r.Breakdown.SectorScore,
                    GeoScore = r.Breakdown.GeoScore,
                    ModelScore = r.Breakdown.ModelScore,
                    StageScore = r.Breakdown.StageScore,
                    FundingBonus = r.Breakdown.FundingBonus,
                    SemanticScore = r.Breakdown.SemanticScore,
                    LlmBonus = r.Breakdown.LlmBonus,
                    AiReason = r.AiReason,
                    EventId = eventId
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    private async Task PersistStartupResultsAsync(int sourceStartupId, List<MatchResultDto> results, int? eventId = null)
    {
        var targetIds = results.Select(r => r.StartupId).ToList();
        var existing = await _db.StartupMatchResults
            .Where(m => m.SourceStartupId == sourceStartupId && targetIds.Contains(m.TargetStartupId))
            .ToDictionaryAsync(m => m.TargetStartupId);

        foreach (var r in results)
        {
            if (existing.TryGetValue(r.StartupId, out var row))
            {
                row.TotalScore = r.Score;
                row.SectorScore = r.Breakdown.SectorScore;
                row.GeoScore = r.Breakdown.GeoScore;
                row.StageScore = r.Breakdown.StageScore;
                row.SemanticScore = r.Breakdown.SemanticScore;
                row.LlmBonus = r.Breakdown.LlmBonus;
                row.AiReason = r.AiReason;
                row.EventId = eventId ?? row.EventId;
                row.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.StartupMatchResults.Add(new StartupMatchResult
                {
                    SourceStartupId = sourceStartupId,
                    TargetStartupId = r.StartupId,
                    TotalScore = r.Score,
                    SectorScore = r.Breakdown.SectorScore,
                    GeoScore = r.Breakdown.GeoScore,
                    StageScore = r.Breakdown.StageScore,
                    SemanticScore = r.Breakdown.SemanticScore,
                    LlmBonus = r.Breakdown.LlmBonus,
                    AiReason = r.AiReason,
                    EventId = eventId
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}

// ── Models for AI Service communication ──

public record SemanticMatchResponse(List<SemanticResult> Results, string Model);

public record SemanticResult(
    int StartupId,
    double SimilarityScore,
    double LlmScore,
    string? Reason
);
