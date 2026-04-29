using MatchingApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MatchingApi.Services;

/// <summary>
/// Background worker that runs periodically to find startups without embeddings 
/// and sends them to the AI microservice for indexing.
/// </summary>
public class StartupIndexingWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<StartupIndexingWorker> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(24);

    public StartupIndexingWorker(IServiceProvider services, ILogger<StartupIndexingWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once immediately on startup
        await ProcessMissingEmbeddingsAsync(stoppingToken);

        using var timer = new PeriodicTimer(_period);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessMissingEmbeddingsAsync(stoppingToken);
        }
    }

    private async Task ProcessMissingEmbeddingsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var aiEngine = scope.ServiceProvider.GetRequiredService<AiMatchingService>();

            // Find startups that haven't been vectorized yet
            var pendingStartups = await db.Startups
                .Where(s => s.Embedding == null)
                .ToListAsync(stoppingToken);

            if (!pendingStartups.Any())
            {
                _logger.LogInformation("No startups pending AI indexing.");
                return;
            }

            _logger.LogInformation("Found {Count} startups pending indexing. Sending to AI Service...", pendingStartups.Count);
            
            await aiEngine.IndexStartupsAsync(pendingStartups);
            
            _logger.LogInformation("Successfully requested indexing for {Count} startups.", pendingStartups.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while indexing startups in background worker.");
        }
    }
}
