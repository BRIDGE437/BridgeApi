using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace BridgeApi.Infrastructure.Services.Mailing;

internal sealed class EmailBackgroundQueue : BackgroundService
{
    private readonly Channel<EmailEnvelope> _channel;
    private readonly IEmailSender _sender;
    private readonly ILogger<EmailBackgroundQueue> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public EmailBackgroundQueue(
        Channel<EmailEnvelope> channel,
        IEmailSender sender,
        ILogger<EmailBackgroundQueue> logger)
    {
        _channel = channel;
        _sender = sender;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                    _logger.LogWarning(ex, "Email send retry {Attempt} in {Delay}s", attempt, delay.TotalSeconds));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundQueue started.");

        await foreach (var envelope in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _retryPolicy.ExecuteAsync(ct => _sender.SendAsync(envelope, ct), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email delivery failed permanently for subject {Subject}", envelope.Subject);
            }
        }

        _logger.LogInformation("EmailBackgroundQueue stopped.");
    }
}
