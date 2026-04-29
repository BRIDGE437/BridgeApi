using MatchingApi.Data;
using MatchingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchingApi.Controllers;

[ApiController]
[Route("api/v1/events")]
public class EventController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Create a new matching event</summary>
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] MatchEvent newEvent)
    {
        _db.MatchEvents.Add(newEvent);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEvent), new { id = newEvent.Id }, newEvent);
    }

    /// <summary>Get a specific matching event</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var evt = await _db.MatchEvents.FindAsync(id);
        if (evt == null) return NotFound();
        return Ok(evt);
    }

    /// <summary>Get all events</summary>
    [HttpGet]
    public async Task<IActionResult> ListEvents()
    {
        var events = await _db.MatchEvents.OrderByDescending(e => e.ScheduledAt).ToListAsync();
        return Ok(events);
    }
}
