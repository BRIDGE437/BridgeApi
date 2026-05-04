using MatchingApi.Data;
using MatchingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchingApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class IndexingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AiMatchingService _aiService;

    public IndexingController(AppDbContext db, AiMatchingService aiService)
    {
        _db = db;
        _aiService = aiService;
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerIndexing([FromQuery] int batchSize = 100)
    {
        var unindexedStartups = await _db.StartupProfiles
            .Where(s => s.Embedding == null)
            .Take(batchSize)
            .ToListAsync();

        if (!unindexedStartups.Any())
            return Ok(new { message = "All startups are already indexed." });

        await _aiService.IndexStartupsAsync(unindexedStartups);
        
        return Ok(new { 
            message = $"Requested indexing for {unindexedStartups.Count} startups.",
            remaining = await _db.StartupProfiles.CountAsync(s => s.Embedding == null)
        });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var total = await _db.StartupProfiles.CountAsync();
        var indexed = await _db.StartupProfiles.CountAsync(s => s.Embedding != null);
        return Ok(new { total, indexed, pending = total - indexed });
    }
}
