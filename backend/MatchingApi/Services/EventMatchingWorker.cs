using MatchingApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MatchingApi.Services;

/// <summary>
/// Background worker that wakes up every minute to check for Open events
/// that are scheduled to run, and executes the matching process.
/// </summary>
public class EventMatchingWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EventMatchingWorker> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

    public EventMatchingWorker(IServiceProvider services, ILogger<EventMatchingWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessEventsAsync(stoppingToken);
        }
    }

    private async Task ProcessEventsAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aiEngine = scope.ServiceProvider.GetRequiredService<AiMatchingService>();

        // Find events that are ready to be processed
        var pendingEvents = await db.MatchEvents
            .Where(e => e.Status == "Open" && e.ScheduledAt <= DateTime.UtcNow)
            .ToListAsync(stoppingToken);

        foreach (var evt in pendingEvents)
        {
            _logger.LogInformation("Starting processing for Event {EventId} - {Title}", evt.Id, evt.Title);
            
            evt.Status = "Processing";
            await db.SaveChangesAsync(stoppingToken);

            try
            {
                // Fetch participants dynamically
                var investors = await db.EventParticipations
                    .Where(p => p.EventId == evt.Id && p.ParticipantType == "Investor")
                    .Select(p => p.ParticipantId)
                    .ToListAsync(stoppingToken);

                var startups = await db.EventParticipations
                    .Where(p => p.EventId == evt.Id && p.ParticipantType == "Startup")
                    .Select(p => int.Parse(p.ParticipantId))
                    .ToListAsync(stoppingToken);

                if (evt.EventType == "Networking")
                {
                    _logger.LogInformation("Event {EventId} is a Networking event. Starting B2B cross-matching...", evt.Id);
                    
                    // Fetch full startup objects for synergy rules
                    var startupsList = await db.Startups
                        .Where(s => startups.Contains(s.Id))
                        .ToListAsync(stoppingToken);

                    foreach (var sourceSid in startups)
                    {
                        await aiEngine.MatchStartupToStartupsAsync(sourceSid, evt.Id, evt.TopMatchingCount, startupsList);
                    }
                }
                else
                {
                    _logger.LogInformation("Event {EventId} is a Standard event. Matching {Count} investors...", evt.Id, investors.Count);
                    
                    // Run standard matching logic for each investor restricted to participating startups
                    foreach (var invId in investors)
                    {
                        await aiEngine.MatchEventAsync(invId, evt.Id, evt.TopMatchingCount, startups);
                    }
                }

                evt.Status = "Completed";
                _logger.LogInformation("Successfully completed Event {EventId}", evt.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Event {EventId}", evt.Id);
            }
            
            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
