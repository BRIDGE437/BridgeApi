using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace BridgeApi.Application.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly HashSet<string> LoggablePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "UserId", "PostId", "ConnectionId", "FollowerId", "FollowingId",
        "SenderId", "ReceiverId", "IntentId"
    };

    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestContext = ExtractRequestContext(request);

        _logger.LogInformation(
            "Handling {RequestName}. Context: {@RequestContext}",
            requestName,
            requestContext);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "{RequestName} completed in {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "{RequestName} failed after {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private static Dictionary<string, object> ExtractRequestContext(TRequest request)
    {
        var context = new Dictionary<string, object>();
        if (request == null) return context;

        var properties = request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (!LoggablePropertyNames.Contains(prop.Name)) continue;
            try
            {
                var value = prop.GetValue(request);
                if (value != null && (value is Guid || value.GetType().IsValueType))
                    context[prop.Name] = value;
            }
            catch { /* ignore reflection errors */ }
        }
        return context;
    }
}
