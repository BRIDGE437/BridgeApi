using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Helpers;
using MatchingApi.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace MatchingApi.Services;

public class StartupSimilarityService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<StartupSimilarityService> _logger;

    // Score weights — base = 85, LLM bonus = up to 15 (sum = 100)
    private const double MaxSector   = 30.0;
    private const double MaxGeo      = 13.0;
    private const double MaxModel    =  8.0;
    private const double MaxSemantic = 34.0;
    private const double MaxLlm      = 15.0;

    public StartupSimilarityService(
        AppDbContext db,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<StartupSimilarityService> logger)
    {
        _db = db;
        _httpClient = httpClientFactory.CreateClient("AiService");
        _config = config;
        _logger = logger;
    }

    public async Task<StartupSimilarityResponseDto> FindSimilarAsync(int startupId, int topN = 10)
    {
        var sw = Stopwatch.StartNew();

        var target = await _db.Startups.FindAsync(startupId)
            ?? throw new KeyNotFoundException($"Startup not found: {startupId}");

        var (candidates, semanticUsed) = await GetCandidatesAsync(target);

        _logger.LogInformation(
            "Startup similarity: target={Id}, candidates={Count}, semantic={Sem}",
            startupId, candidates.Count, semanticUsed);

        // Pre-parse target fields once — avoid re-parsing per candidate in the hot loop
        var targetTags    = target.ParsedTags;
        var targetModels  = target.ParsedBusinessModels;
        var (targetCity, targetCountry) = target.ParsedHQ;
        var targetCities  = new List<string> { targetCity };
        var targetRegions = new List<string> { targetCountry, RegionMapper.GetRegion(targetCountry) };

        // Base scoring
        var scored = candidates
            .Select(c => ScoreCandidate(c.Startup, c.SemanticScore, semanticUsed,
                targetTags, targetModels, targetCities, targetRegions))
            .OrderByDescending(r => r.Score)
            .Take(topN * 2) // take extra buffer before LLM
            .ToList();

        // LLM bonus on top N
        var llmUsed = false;
        var useLlm = _config.GetValue<bool>("AiService:UseLlm", false);
        if (useLlm && semanticUsed)
        {
            try
            {
                var targetText = BuildStartupText(target);
                var top = scored.Take(topN).ToList();
                var llmScores = await GetLlmScoresAsync(targetText, top, candidates);

                scored = scored.Select(r =>
                {
                    if (!llmScores.TryGetValue(r.StartupId, out var llm)) return r;
                    llmUsed = true;
                    var bonus = Math.Round(llm.Score / 10.0 * MaxLlm, 1);
                    return r with
                    {
                        LlmBonus = bonus,
                        AiReason = llm.Reason,
                        Score = Math.Round(Math.Min(100, r.Score + bonus), 1),
                    };
                }).OrderByDescending(r => r.Score).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LLM scoring failed for startup similarity; skipping");
            }
        }

        var finalResults = scored
            .Take(topN)
            .Select((r, i) => r with { Rank = i + 1 })
            .ToList();

        sw.Stop();
        return new StartupSimilarityResponseDto(
            TargetStartupId:   target.Id,
            TargetStartupName: target.Name,
            TotalCandidates:   candidates.Count,
            Results:           finalResults,
            ProcessingTimeMs:  sw.ElapsedMilliseconds,
            SemanticUsed:      semanticUsed,
            LlmUsed:           llmUsed
        );
    }

    // ── Candidate retrieval ──────────────────────────────────────────────────

    private async Task<(List<(Startup Startup, double SemanticScore)> Candidates, bool SemanticUsed)>
        GetCandidatesAsync(Startup target)
    {
        if (target.Embedding != null)
        {
            var rows = await _db.Startups
                .Where(s => s.Id != target.Id && s.Status == "Alive" && s.Embedding != null)
                .OrderBy(s => s.Embedding!.CosineDistance(target.Embedding))
                .Take(200)
                .ToListAsync();

            var targetVec = target.Embedding.ToArray();
            var candidates = rows
                .Select(s => (
                    Startup: s,
                    SemanticScore: CosineSimilarity(targetVec, s.Embedding!.ToArray())
                ))
                .ToList();

            return (candidates, true);
        }

        _logger.LogWarning("Target startup {Id} has no embedding; using rule-based only", target.Id);
        var all = await _db.Startups
            .Where(s => s.Id != target.Id && s.Status == "Alive")
            .ToListAsync();

        return (all.Select(s => (s, 0.0)).ToList(), false);
    }

    // ── LLM scoring ──────────────────────────────────────────────────────────

    private async Task<Dictionary<int, (double Score, string? Reason)>> GetLlmScoresAsync(
        string targetText,
        List<StartupSimilarityResultDto> topCandidates,
        List<(Startup Startup, double SemanticScore)> allCandidates)
    {
        var candidateMap = allCandidates.ToDictionary(c => c.Startup.Id, c => c.Startup);

        var request = new
        {
            investor_text = targetText,
            startups = topCandidates.Select(c =>
            {
                var s = candidateMap[c.StartupId];
                return new { id = c.StartupId, text = BuildStartupText(s) };
            }).ToList(),
            use_llm = true,
            mode = "startup_similarity",
        };

        var response = await _httpClient.PostAsJsonAsync("/api/v1/semantic-match", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SemanticMatchResponse>(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
            });

        return (result?.Results ?? new())
            .Where(r => r.LlmScore > 0)
            .ToDictionary(r => r.StartupId, r => (r.LlmScore, r.Reason));
    }

    private static string BuildStartupText(Startup s) =>
        $"{s.Name}. {s.Tags ?? ""}. {s.Description ?? ""}. {s.BusinessModel ?? ""}. {s.HQ ?? ""}";

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot   += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        if (normA == 0 || normB == 0) return 0;
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    // ── Scoring ──────────────────────────────────────────────────────────────

    private static StartupSimilarityResultDto ScoreCandidate(
        Startup candidate, double cosine, bool semanticUsed,
        List<string> targetTags, List<string> targetModels,
        List<string> targetCities, List<string> targetRegions)
    {
        double sectorScore = SectorSimilarity.CalculateSectorScore(
            candidate.ParsedTags, targetTags, MaxSector);

        var (cCity, cCountry) = candidate.ParsedHQ;
        double geoScore = RegionMapper.CalculateGeoScore(
            cCity, cCountry, targetCities, targetRegions, MaxGeo);

        double modelScore = RuleBasedMatchingService.CalculateBusinessModelScore(
            candidate.ParsedBusinessModels, targetModels, MaxModel);

        double semanticScore = semanticUsed
            ? Math.Round(Math.Max(0, cosine) * MaxSemantic, 1)
            : 0;

        double total = Math.Round(sectorScore + geoScore + modelScore + semanticScore, 1);

        return new StartupSimilarityResultDto(
            Rank:          0,
            StartupId:     candidate.Id,
            StartupName:   candidate.Name,
            Score:         total,
            SectorScore:   Math.Round(sectorScore, 1),
            GeoScore:      Math.Round(geoScore, 1),
            ModelScore:    Math.Round(modelScore, 1),
            SemanticScore: semanticScore,
            LlmBonus:      0,
            AiReason:      null,
            Tags:          candidate.ParsedTags,
            HQ:            candidate.HQ,
            BusinessModel: candidate.BusinessModel,
            Description:   candidate.Description,
            Stage:         candidate.Stage,
            Website:       candidate.Website
        );
    }
}
