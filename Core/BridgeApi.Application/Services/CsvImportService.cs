using System.Globalization;
using BridgeApi.Application.Abstractions;
using BridgeApi.Domain.Entities;
using BridgeApi.Shared.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Services;

/// <summary>
/// Service responsible for importing and processing startup data from CSV files.
/// Implements robust deduplication using a Fingerprint system based on Website and Company Name.
/// </summary>
public class CsvImportService : ICsvImportService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(
        UserManager<AppUser> userManager,
        IApplicationDbContext context,
        ILogger<CsvImportService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<(int success, int failed, string message)> ImportStartupsAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (0, 0, "File is empty.");

        int successCount = 0;
        int failedCount = 0;

        _logger.LogInformation("Starting CSV Import: {FileName}", file.FileName);

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace(" ", "").Replace("/", "").Replace("_", "")
            });

            var records = csv.GetRecords<dynamic>().ToList();
            int totalRecords = records.Count;
            _logger.LogInformation("Found {Count} records in CSV.", totalRecords);

            if (records.Count > 0)
            {
                var first = (IDictionary<string, object>)records[0];
                _logger.LogInformation("DIAGNOSTIC - Actual Keys in Dictionary: {Keys}", string.Join("|", first.Keys));
            }

            int current = 0;
            var errorLog = new List<string> { "Row,StartupName,Email,Error" };

            // Column Index Mapping (Zero-based)
            const int IDX_ID = 0;
            const int IDX_NAME = 1;
            const int IDX_WEBSITE = 2;
            const int IDX_DESC = 6;
            const int IDX_HQ = 8;
            const int IDX_TAGS = 10;
            const int IDX_BIZ_MODEL = 11;
            const int IDX_REV_MODEL = 12;
            const int IDX_REV_STATE = 13;
            const int IDX_FUNDING = 14;
            const int IDX_STAGE = 15;
            const int IDX_EMAIL = 16;
            const int IDX_WEB_DESC = 17;

            // Use the already loaded records list
            foreach (var record in records)
            {
                current++;
                string name = "Unknown";
                string email = "Unknown";
                
                try
                {
                    var dict = (IDictionary<string, object>)record;
                    var values = dict.Values.ToList();

                    // Access fields by INDEX safely from the dictionary values
                    string csvId = values.Count > IDX_ID ? values[IDX_ID]?.ToString()?.Trim() : null;
                    name = (values.Count > IDX_NAME ? values[IDX_NAME]?.ToString()?.Trim() : null) ?? "Unknown Startup";
                    
                    if (string.IsNullOrWhiteSpace(csvId))
                    {
                        _logger.LogWarning("Row {Current}: ID is missing at Index {Idx}. Skipping.", current, IDX_ID);
                        continue;
                    }

                    // Handle multiple emails and ensure uniqueness for Identity
                    string allEmails = values.Count > IDX_EMAIL ? values[IDX_EMAIL]?.ToString()?.Trim() : "";
                    string primaryEmail = allEmails?.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? $"startup_{csvId}@bridge.com";
                    
                    if (current % 100 == 0)
                        _logger.LogInformation("[{Current}/{Total}] Processing: {Name}", current, totalRecords, name);

                    // 1. Generate Fingerprint (Website + Name)
                    string website = (values.Count > IDX_WEBSITE ? values[IDX_WEBSITE]?.ToString()?.Trim() : "") ?? "";
                    string fingerprint = GenerateFingerprint(name, website);

                    // 2. Find existing profile by its unique fingerprint
                    var profile = await _context.StartupProfiles
                        .FirstOrDefaultAsync(p => p.ExternalFingerprint == fingerprint);

                    AppUser user = null;
                    if (profile != null)
                    {
                        user = await _userManager.FindByIdAsync(profile.UserId);
                    }
                    
                    // IF we haven't found a profile yet, check if this email is already taken
                    bool needsReview = false;

                    // Automatically flag known garbage emails from scrapers
                    if (primaryEmail.Contains("sentry.io") || primaryEmail.Contains("wixpress.com"))
                    {
                        needsReview = true;
                        primaryEmail = $"pending_review_{csvId}@bridge.com";
                    }

                    if (user == null)
                    {
                        var existingUser = await _userManager.FindByEmailAsync(primaryEmail);
                        if (existingUser != null)
                        {
                            // If this email is taken by ANOTHER startup (different fingerprint),
                            // we must make our primaryEmail unique for this new startup.
                            needsReview = true;
                            primaryEmail = MakeUniqueEmail(primaryEmail, csvId);
                            user = await _userManager.FindByEmailAsync(primaryEmail);
                        }
                        else
                        {
                            // Email is free, we can use it
                            user = null; 
                        }
                    }

                    if (user == null)
                    {
                        user = new AppUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserName = primaryEmail,
                            Email = primaryEmail,
                            AuthProvider = "CSV_Import",
                            CreatedAt = DateTime.UtcNow
                        };

                        var result = await _userManager.CreateAsync(user, "Startup123!");
                        if (!result.Succeeded)
                        {
                            _logger.LogWarning("User creation failed for {Name}: {Errors}", name, string.Join("|", result.Errors.Select(e => e.Description)));
                            errorLog.Add($"{current},{name.Replace(",", " ")},{primaryEmail},User creation failed");
                            failedCount++;
                            continue;
                        }
                    }

                    // 3. Create or Update StartupProfile
                    if (profile == null)
                        profile = await _context.StartupProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    
                    if (profile == null)
                    {
                        profile = new StartupProfile { UserId = user.Id };
                        _context.StartupProfiles.Add(profile);
                    }

                    // Update fields and set Fingerprint
                    profile.ExternalFingerprint = fingerprint;
                    profile.NeedsManualReview = needsReview;
                    profile.ContactEmails = Truncate(allEmails, 2000);
                    profile.CompanyName = Truncate(name, 200);
                    profile.WebsiteUrl = Truncate(website, 500);
                    profile.Description = Truncate(values.Count > IDX_DESC ? values[IDX_DESC]?.ToString() : null, 5000);
                    profile.HQ = Truncate(values.Count > IDX_HQ ? values[IDX_HQ]?.ToString() : null, 200);
                    profile.Tags = Truncate((values.Count > IDX_TAGS ? values[IDX_TAGS]?.ToString() : null) ?? "startup", 1000);
                    profile.BusinessModel = Truncate((values.Count > IDX_BIZ_MODEL ? values[IDX_BIZ_MODEL]?.ToString() : null) ?? "b2b", 100);
                    profile.RevenueModel = Truncate(values.Count > IDX_REV_MODEL ? values[IDX_REV_MODEL]?.ToString() : null, 100);
                    profile.RevenueState = Truncate(values.Count > IDX_REV_STATE ? values[IDX_REV_STATE]?.ToString() : null, 100);
                    profile.Stage = Truncate((values.Count > IDX_STAGE ? values[IDX_STAGE]?.ToString() : null) ?? "seed", 100);
                    profile.WebsiteDescription = Truncate(values.Count > IDX_WEB_DESC ? values[IDX_WEB_DESC]?.ToString() : null, 5000);

                    string fundingStr = values.Count > IDX_FUNDING ? values[IDX_FUNDING]?.ToString() : null;
                    if (!string.IsNullOrEmpty(fundingStr))
                    {
                        long.TryParse(new string(fundingStr.Where(char.IsDigit).ToArray()), out long totalFunding);
                        profile.TotalFunding = totalFunding;
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to import row {Current}", current);
                    errorLog.Add($"{current},{name.Replace(",", " ")},{email},{ex.Message.Replace(",", ";")}");
                    failedCount++;
                }

                if (current % 100 == 0)
                    await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();

            // Save error report
            if (errorLog.Count > 1)
            {
                string errorFilePath = Path.Combine(Directory.GetCurrentDirectory(), "import_errors.csv");
                await File.WriteAllLinesAsync(errorFilePath, errorLog);
                _logger.LogInformation("Error report saved to: {Path}", errorFilePath);
            }
            _logger.LogInformation("Import Finished. Total: {Total}, Success: {Success}, Failed: {Failed}", totalRecords, successCount, failedCount);
            return (successCount, failedCount, $"Import completed. Success: {successCount}, Failed: {failedCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CSV Import failed");
            return (0, 0, $"Error reading CSV: {ex.Message}");
        }
    }

    public async Task<string> ClearImportedDataAsync()
    {
        _logger.LogInformation("Executing massive cleanup (TRUNCATE CASCADE)...");
        
        // Raw SQL for absolute cleanup in PostgreSQL
        var sql = @"
            TRUNCATE TABLE ""Follows"", ""PostComments"", ""PostLikes"", ""Posts"", 
                           ""Messages"", ""Connections"", ""UserIntents"", ""Intents"", 
                           ""StartupProfiles"", ""InvestorProfiles"", ""UserProfiles"", 
                           ""StoredFiles"", ""AspNetUserRoles"", ""AspNetUserClaims"", 
                           ""AspNetUserLogins"", ""AspNetUserTokens"", ""AspNetUsers"" 
            RESTART IDENTITY CASCADE;";

        try 
        {
            if (_context is DbContext dbContext)
            {
                await dbContext.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation("Database has been fully reset.");
                return "Database fully cleared. All tables truncated.";
            }
            return "Failed to access database context for truncate.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to truncate tables.");
            return $"Error during cleanup: {ex.Message}";
        }
    }
    
    public async Task<(int userCount, int profileCount)> GetImportStatsAsync()
    {
        var userCount = await _context.Users.CountAsync(u => u.AuthProvider == "CSV_Import");
        var profileCount = await _context.StartupProfiles.CountAsync();
        return (userCount, profileCount);
    }

    /// <summary>
    /// Generates a unique MD5 fingerprint based on the normalized website and company name.
    /// This prevents duplicate records when the same startup appears multiple times in the CSV.
    /// </summary>
    private string MakeUniqueEmail(string email, string csvId)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains("@")) 
            return $"startup_{csvId}@bridge.com";

        var parts = email.Split('@');
        string prefix = parts[0];
        string domain = parts[1];

        return $"{prefix}+{csvId}@{domain}";
    }

    private string GenerateFingerprint(string name, string website)
    {
        string normalizedUrl = NormalizeUrl(website);
        // Normalize name: lowercase and remove non-alphanumeric characters
        string normalizedName = new string((name ?? "").ToLowerInvariant().Where(c => char.IsLetterOrDigit(c)).ToArray());
        
        string raw = $"{normalizedUrl}|{normalizedName}";
        
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(raw);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    private string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "no-website";
        
        string clean = url.ToLowerInvariant().Trim();
        
        // Remove protocols
        if (clean.StartsWith("https://")) clean = clean.Substring(8);
        if (clean.StartsWith("http://")) clean = clean.Substring(7);
        
        // Remove www.
        if (clean.StartsWith("www.")) clean = clean.Substring(4);
        
        // Remove trailing slash
        if (clean.EndsWith("/")) clean = clean.Substring(0, clean.Length - 1);
        
        // Take only domain part if there are subpages
        int slashIndex = clean.IndexOf('/');
        if (slashIndex > 0) clean = clean.Substring(0, slashIndex);
        
        return clean;
    }

    private string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    private string GetVal(IDictionary<string, object> dict, string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        
        // Normalize the search key (lowercase, no spaces/underscores)
        string normSearch = key.ToLowerInvariant().Trim().Replace(" ", "").Replace("_", "").Replace("/", "");
        
        var entry = dict.FirstOrDefault(x => {
            // Normalize the dictionary key (CSV column name)
            string normKey = x.Key.ToLowerInvariant().Trim().Replace(" ", "").Replace("_", "").Replace("/", "");
            
            // Remove UTF-8 BOM if it exists in the first column
            if (normKey.StartsWith("\ufeff")) normKey = normKey.Substring(1);
            
            return normKey == normSearch;
        });

        return entry.Value?.ToString()?.Trim();
    }
}
