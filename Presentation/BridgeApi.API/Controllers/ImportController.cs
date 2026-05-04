using BridgeApi.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace BridgeApi.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ImportController : ControllerBase
{
    private readonly ICsvImportService _csvImportService;

    public ImportController(ICsvImportService csvImportService)
    {
        _csvImportService = csvImportService;
    }

    [HttpPost("startups")]
    public async Task<IActionResult> ImportStartups(IFormFile file)
    {
        var (success, failed, message) = await _csvImportService.ImportStartupsAsync(file);
        
        if (success == 0 && failed > 0)
            return BadRequest(new { message });
            
        return Ok(new { success, failed, message });
    }
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var (userCount, profileCount) = await _csvImportService.GetImportStatsAsync();
        return Ok(new { totalImportedUsers = userCount, totalStartupProfiles = profileCount });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearData()
    {
        var result = await _csvImportService.ClearImportedDataAsync();
        return Ok(new { message = result });
    }
}
