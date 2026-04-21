using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Models;
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

    /// <summary>Index startups via JSON directly.</summary>
    [HttpPost("index-startups")]
    public async Task<IActionResult> IndexStartups([FromBody] List<Startup> startups)
    {
        if (startups == null || !startups.Any())
            return BadRequest(new { error = "No startups provided." });

        try
        {
            var addedCount = 0;
            var updatedCount = 0;

            foreach (var s in startups)
            {
                var existing = await _db.Startups.FindAsync(s.Id);
                if (existing != null)
                {
                    _db.Entry(existing).CurrentValues.SetValues(s);
                    updatedCount++;
                }
                else
                {
                    _db.Startups.Add(s);
                    addedCount++;
                }
            }

            await _db.SaveChangesAsync();

            // Trigger vectorization in Python AI service
            await _aiEngine.IndexStartupsAsync(startups);

            return Ok(new { message = "Startups successfully indexed.", added = addedCount, updated = updatedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred during indexing.", details = ex.Message });
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
        [FromQuery] int? eventId = null,
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
            
        if (eventId.HasValue)
            query = query.Where(m => m.EventId == eventId.Value);

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
                m.EventId,
                m.CreatedAt,
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = results });
    }

    /// <summary>Join an event</summary>
    [HttpPost("event/{eventId}/join")]
    public async Task<IActionResult> JoinEvent(int eventId, [FromQuery] string participantId, [FromQuery] string participantType)
    {
        var evt = await _db.MatchEvents.FindAsync(eventId);
        if (evt == null) return NotFound(new { error = "Event not found" });

        if (evt.Status != "Upcoming" && evt.Status != "Open")
            return BadRequest(new { error = "Event is no longer open for participation." });

        // Validate participant
        if (participantType == "Investor")
        {
            if (!await _db.Investors.AnyAsync(i => i.InvestorId == participantId))
                return NotFound(new { error = "Investor not found" });
        }
        else if (participantType == "Startup")
        {
            if (!int.TryParse(participantId, out int sid) || !await _db.Startups.AnyAsync(s => s.Id == sid))
                return NotFound(new { error = "Startup not found" });
        }
        else
        {
            return BadRequest(new { error = "Invalid participant type. Must be Investor or Startup." });
        }

        var participation = new EventParticipation
        {
            EventId = eventId,
            ParticipantId = participantId,
            ParticipantType = participantType
        };

        try
        {
            _db.EventParticipations.Add(participation);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Successfully joined the event." });
        }
        catch (DbUpdateException)
        {
            return BadRequest(new { error = "Already participating in this event." });
        }
    }

    /// <summary>Leave an event</summary>
    [HttpDelete("event/{eventId}/leave")]
    public async Task<IActionResult> LeaveEvent(int eventId, [FromQuery] string participantId, [FromQuery] string participantType)
    {
        var participation = await _db.EventParticipations
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.ParticipantId == participantId && p.ParticipantType == participantType);

        if (participation == null) return NotFound(new { error = "Participation not found" });

        var evt = await _db.MatchEvents.FindAsync(eventId);
        if (evt != null && evt.Status != "Upcoming" && evt.Status != "Open")
            return BadRequest(new { error = "Cannot leave an event that is currently processing or completed." });

        _db.EventParticipations.Remove(participation);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Successfully left the event." });
    }

    /// <summary>Run hybrid matching for an investor restricted to event participants.</summary>
    [HttpPost("event/match-investor")]
    public async Task<ActionResult<MatchResponseDto>> MatchEventInvestor(string investorId, int eventId, int topN = 10)
    {
        var participantIds = await _db.EventParticipations
            .Where(p => p.EventId == eventId && p.ParticipantType == "Startup")
            .Select(p => p.ParticipantId)
            .ToListAsync();

        var startupIdsInt = participantIds
            .Select(id => int.TryParse(id, out int sid) ? sid : 0)
            .Where(id => id > 0)
            .ToList();

        if (!startupIdsInt.Any())
            return BadRequest(new { error = "No startups participating in this event" });

        var results = await _aiEngine.MatchEventAsync(investorId, eventId, topN, startupIdsInt);
        return Ok(results);
    }

    /// <summary>Run B2B synergy matching between startups in an event.</summary>
    [HttpPost("event/match-startup")]
    public async Task<ActionResult<MatchResponseDto>> MatchEventStartup(int sourceStartupId, int eventId, int topN = 10)
    {
        var participantIds = await _db.EventParticipations
            .Where(p => p.EventId == eventId && p.ParticipantType == "Startup" && p.ParticipantId != sourceStartupId.ToString())
            .Select(p => p.ParticipantId)
            .ToListAsync();

        var participantIdsInt = participantIds
            .Select(id => int.TryParse(id, out int sid) ? sid : 0)
            .Where(id => id > 0)
            .ToList();

        var startups = await _db.Startups
            .Where(s => participantIdsInt.Contains(s.Id))
            .ToListAsync();

        if (!startups.Any())
            return BadRequest(new { error = "No other startups participating in this event for B2B matching" });

        var results = await _aiEngine.MatchStartupToStartupsAsync(sourceStartupId, eventId, topN, startups);
        return Ok(results);
    }

    /// <summary>Retrieves the B2B matching history for a specific startup.</summary>
    [HttpGet("history-b2b")]
    public async Task<ActionResult> GetB2BHistory(int startupId, int? eventId = null, int topN = 50)
    {
        var query = _db.StartupMatchResults.AsQueryable();
        query = query.Where(m => m.SourceStartupId == startupId);

        if (eventId.HasValue)
            query = query.Where(m => m.EventId == eventId.Value);

        var results = await query
            .Include(m => m.TargetStartup)
            .OrderByDescending(m => m.CreatedAt)
            .Take(topN)
            .ToListAsync();

        return Ok(results);
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
