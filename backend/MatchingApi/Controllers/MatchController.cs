using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchingApi.Controllers;

[ApiController]
[Route("api/v1/match")]
public class MatchController : ControllerBase
{
    private readonly RuleBasedMatchingService _ruleEngine;
    private readonly AiMatchingService _aiEngine;
    private readonly AppDbContext _db;

    public MatchController(
        RuleBasedMatchingService ruleEngine,
        AiMatchingService aiEngine,
        AppDbContext db)
    {
        _ruleEngine = ruleEngine;
        _aiEngine = aiEngine;
        _db = db;
    }

    /// <summary>Run rule-based matching for an investor.</summary>
    [HttpPost("rule-based")]
    public async Task<IActionResult> RuleBasedMatch([FromBody] MatchRequest request)
    {
        try
        {
            var result = await _ruleEngine.MatchAsync(request.InvestorId, request.TopN);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>Run AI-powered hybrid matching for an investor.</summary>
    [HttpPost("ai-powered")]
    public async Task<IActionResult> AiPoweredMatch([FromBody] MatchRequest request)
    {
        try
        {
            var result = await _aiEngine.MatchAsync(request.InvestorId, request.TopN);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new
            {
                error = "AI service is unavailable. Falling back to rule-based matching.",
                details = ex.Message
            });
        }
    }

    /// <summary>Get match history for an investor.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? investorId = null,
        [FromQuery] string? mode = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.MatchResults
            .Include(m => m.Startup)
            .Include(m => m.Investor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(investorId))
            query = query.Where(m => m.InvestorId == investorId);

        if (!string.IsNullOrEmpty(mode))
            query = query.Where(m => m.MatchingMode == mode);

        var total = await query.CountAsync();

        var results = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.InvestorId,
                InvestorName = m.Investor != null ? m.Investor.Name : "",
                m.StartupId,
                StartupName = m.Startup != null ? m.Startup.Name : "",
                m.MatchingMode,
                m.TotalScore,
                m.SectorScore,
                m.GeoScore,
                m.ModelScore,
                m.StageScore,
                m.SemanticScore,
                m.LlmBonus,
                m.AiReason,
                m.CreatedAt,
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = results });
    }

    /// <summary>Compare both matching modes side by side.</summary>
    [HttpPost("compare")]
    public async Task<IActionResult> Compare([FromBody] MatchRequest request)
    {
        try
        {
            var ruleTask = _ruleEngine.MatchAsync(request.InvestorId, request.TopN);

            MatchResponseDto? aiResult = null;
            try
            {
                aiResult = await _aiEngine.MatchAsync(request.InvestorId, request.TopN);
            }
            catch
            {
                // AI service unavailable, continue with rule-based only
            }

            var ruleResult = await ruleTask;

            return Ok(new
            {
                ruleBased = ruleResult,
                aiPowered = aiResult,
                comparison = new
                {
                    ruleBasedAvgScore = ruleResult.Results.Any()
                        ? Math.Round(ruleResult.Results.Average(r => r.Score), 1) : 0,
                    aiPoweredAvgScore = aiResult?.Results.Any() == true
                        ? Math.Round(aiResult.Results.Average(r => r.Score), 1) : 0,
                    commonStartups = aiResult != null
                        ? ruleResult.Results
                            .Select(r => r.StartupId)
                            .Intersect(aiResult.Results.Select(r => r.StartupId))
                            .Count() : 0,
                }
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
