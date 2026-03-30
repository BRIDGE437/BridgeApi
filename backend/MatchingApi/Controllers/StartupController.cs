using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchingApi.Controllers;

[ApiController]
[Route("api/v1/startups")]
public class StartupController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CsvImportService _importService;
    private readonly StartupSimilarityService _similarityService;

    public StartupController(AppDbContext db, CsvImportService importService, StartupSimilarityService similarityService)
    {
        _db = db;
        _importService = importService;
        _similarityService = similarityService;
    }

    /// <summary>List startups with optional filtering and pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = "Alive",
        [FromQuery] string? tags = null,
        [FromQuery] string? hq = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Startups.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(s => s.Status == status);

        if (!string.IsNullOrEmpty(tags))
            query = query.Where(s => s.Tags != null && s.Tags.Contains(tags));

        if (!string.IsNullOrEmpty(hq))
            query = query.Where(s => s.HQ != null && s.HQ.Contains(hq));

        var total = await query.CountAsync();

        var startups = await query
            .OrderByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StartupDto(
                s.Id, s.Name, s.Website, s.Status, s.Description,
                s.YearFounded, s.HQ, s.Founders,
                new List<string>(), new List<string>(),
                s.RevenueModel, s.RevenueState, s.TotalFunding, s.Stage))
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = startups });
    }

    /// <summary>Get a single startup by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _db.Startups.FindAsync(id);
        if (s == null) return NotFound();

        return Ok(new StartupDto(
            s.Id, s.Name, s.Website, s.Status, s.Description,
            s.YearFounded, s.HQ, s.Founders,
            s.ParsedTags, s.ParsedBusinessModels,
            s.RevenueModel, s.RevenueState, s.TotalFunding, s.Stage));
    }

    /// <summary>Import startups from CSV file.</summary>
    [HttpPost("import")]
    [RequestSizeLimit(50_000_000)] // 50MB
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("CSV file is required");

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only CSV files are accepted");

        using var stream = file.OpenReadStream();
        var result = await _importService.ImportStartupsAsync(stream);

        return Ok(result);
    }

    /// <summary>Get unique tag values for filtering UI.</summary>
    [HttpGet("tags")]
    public async Task<IActionResult> GetAllTags()
    {
        var startups = await _db.Startups
            .Where(s => s.Tags != null && s.Status == "Alive")
            .Select(s => s.Tags)
            .ToListAsync();

        var tags = startups
            .Where(t => t != null)
            .SelectMany(t => t!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return Ok(tags);
    }

    /// <summary>Find startups similar to the given startup.</summary>
    [HttpGet("{id:int}/similar")]
    public async Task<IActionResult> GetSimilar(int id, [FromQuery] int topN = 10)
    {
        try
        {
            var result = await _similarityService.FindSimilarAsync(id, topN);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Get startup count stats.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var total = await _db.Startups.CountAsync();
        var alive = await _db.Startups.CountAsync(s => s.Status == "Alive");
        var withTags = await _db.Startups.CountAsync(s => s.Tags != null);

        return Ok(new { total, alive, withTags });
    }
}
