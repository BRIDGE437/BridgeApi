using CsvHelper;
using CsvHelper.Configuration;
using MatchingApi.Data;
using MatchingApi.DTOs;
using MatchingApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MatchingApi.Services;

public class CsvImportService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(AppDbContext db, ILogger<CsvImportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportStartupsAsync(Stream csvStream)
    {
        var errors = new List<string>();
        int imported = 0, skipped = 0, totalRows = 0;

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            PrepareHeaderForMatch = args => args.Header.Replace(" ", "_").Replace("/", "_"),
            BadDataFound = context =>
            {
                _logger.LogWarning("Bad CSV data at row {Row}", context.RawRecord);
            }
        });

        var records = csv.GetRecords<StartupCsvRow>().ToList();
        totalRows = records.Count;

        foreach (var row in records)
        {
            try
            {
                // Check if already exists
                var existing = await _db.Startups.FindAsync(row.ID);
                if (existing != null)
                {
                    // Update existing
                    UpdateStartup(existing, row);
                    skipped++;
                    continue;
                }

                var startup = new Startup
                {
                    Id = row.ID,
                    Name = row.Name ?? $"Unknown_{row.ID}",
                    Website = NullIfEmpty(row.Website),
                    Twitter = NullIfEmpty(row.Twitter),
                    Instagram = NullIfEmpty(row.Instagram),
                    Status = row.Status ?? "Unknown",
                    Description = NullIfEmpty(row.Description),
                    YearFounded = ParseYear(row.Year_Founded),
                    HQ = NullIfEmpty(row.HQ),
                    Founders = NullIfEmpty(row.Founder_People),
                    Tags = NullIfEmpty(row.Tags),
                    BusinessModel = NullIfEmpty(row.Business_Model),
                    RevenueModel = NullIfEmpty(row.Revenue_Model),
                    RevenueState = NullIfEmpty(row.Revenue_State),
                    TotalFunding = NullIfEmpty(row.Total_Funding),
                    Stage = NullIfEmpty(row.Stage),
                    WebsiteEmail = NullIfEmpty(row.Website_Email),
                    WebsiteDescription = NullIfEmpty(row.Website_Description),
                };

                _db.Startups.Add(startup);
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row.ID}: {ex.Message}");
                _logger.LogError(ex, "Error importing row {Id}", row.ID);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Import complete: {Imported} imported, {Skipped} updated, {Errors} errors",
            imported, skipped, errors.Count);

        return new ImportResultDto(totalRows, imported, skipped, errors);
    }

    private static void UpdateStartup(Startup existing, StartupCsvRow row)
    {
        existing.Name = row.Name ?? existing.Name;
        existing.Website = NullIfEmpty(row.Website) ?? existing.Website;
        existing.Status = row.Status ?? existing.Status;
        existing.Description = NullIfEmpty(row.Description) ?? existing.Description;
        existing.HQ = NullIfEmpty(row.HQ) ?? existing.HQ;
        existing.Tags = NullIfEmpty(row.Tags) ?? existing.Tags;
        existing.BusinessModel = NullIfEmpty(row.Business_Model) ?? existing.BusinessModel;
        existing.RevenueState = NullIfEmpty(row.Revenue_State) ?? existing.RevenueState;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseYear(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return int.TryParse(value.Trim(), out var year) ? year : null;
    }
}

/// <summary>Maps CSV columns to properties.</summary>
public class StartupCsvRow
{
    public int ID { get; set; }
    public string? Name { get; set; }
    public string? Website { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public string? Year_Founded { get; set; }
    public string? HQ { get; set; }
    public string? Founder_People { get; set; }
    public string? Tags { get; set; }
    public string? Business_Model { get; set; }
    public string? Revenue_Model { get; set; }
    public string? Revenue_State { get; set; }
    public string? Total_Funding { get; set; }
    public string? Stage { get; set; }
    public string? Website_Email { get; set; }
    public string? Website_Description { get; set; }
}
