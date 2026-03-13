using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchingApi.Controllers;

[ApiController]
[Route("api/v1/investors")]
public class InvestorController : ControllerBase
{
    private readonly AppDbContext _db;

    public InvestorController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>List all investors.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? active = null)
    {
        var query = _db.Investors.AsQueryable();
        if (active.HasValue)
            query = query.Where(i => i.Active == active.Value);

        var investors = await query.OrderBy(i => i.Name).ToListAsync();
        return Ok(investors);
    }

    /// <summary>Get investor by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var investor = await _db.Investors.FindAsync(id);
        if (investor == null) return NotFound();
        return Ok(investor);
    }

    /// <summary>Create a new investor.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InvestorCreateDto dto)
    {
        var investor = new Investor
        {
            InvestorId = $"inv_{Guid.NewGuid().ToString("N")[..8]}",
            Name = dto.Name,
            Type = dto.Type,
            PreferredSectors = dto.PreferredSectors,
            PreferredBusinessModel = dto.PreferredBusinessModel,
            PreferredRegions = dto.PreferredRegions,
            PreferredCities = dto.PreferredCities,
            InvestmentStage = dto.InvestmentStage,
            TicketSizeMin = dto.TicketSizeMin,
            TicketSizeMax = dto.TicketSizeMax,
            PreferredRevenueState = dto.PreferredRevenueState,
            Portfolio = dto.Portfolio,
            Description = dto.Description,
            Website = dto.Website,
            ContactEmail = dto.ContactEmail,
            LinkedIn = dto.LinkedIn,
            Active = true,
        };

        _db.Investors.Add(investor);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = investor.InvestorId }, investor);
    }

    /// <summary>Update an existing investor.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] InvestorUpdateDto dto)
    {
        var investor = await _db.Investors.FindAsync(id);
        if (investor == null) return NotFound();

        if (dto.Name != null) investor.Name = dto.Name;
        if (dto.Type != null) investor.Type = dto.Type;
        if (dto.PreferredSectors != null) investor.PreferredSectors = dto.PreferredSectors;
        if (dto.PreferredBusinessModel != null) investor.PreferredBusinessModel = dto.PreferredBusinessModel;
        if (dto.PreferredRegions != null) investor.PreferredRegions = dto.PreferredRegions;
        if (dto.PreferredCities != null) investor.PreferredCities = dto.PreferredCities;
        if (dto.InvestmentStage != null) investor.InvestmentStage = dto.InvestmentStage;
        if (dto.TicketSizeMin.HasValue) investor.TicketSizeMin = dto.TicketSizeMin.Value;
        if (dto.TicketSizeMax.HasValue) investor.TicketSizeMax = dto.TicketSizeMax.Value;
        if (dto.PreferredRevenueState != null) investor.PreferredRevenueState = dto.PreferredRevenueState;
        if (dto.Portfolio != null) investor.Portfolio = dto.Portfolio;
        if (dto.Description != null) investor.Description = dto.Description;
        if (dto.Website != null) investor.Website = dto.Website;
        if (dto.ContactEmail != null) investor.ContactEmail = dto.ContactEmail;
        if (dto.LinkedIn != null) investor.LinkedIn = dto.LinkedIn;
        if (dto.Active.HasValue) investor.Active = dto.Active.Value;

        await _db.SaveChangesAsync();
        return Ok(investor);
    }

    /// <summary>Delete an investor.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var investor = await _db.Investors.FindAsync(id);
        if (investor == null) return NotFound();

        _db.Investors.Remove(investor);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
