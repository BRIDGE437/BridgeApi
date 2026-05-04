using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.RateLimiting;
using BridgeApi.API.Models;

namespace BridgeApi.API.Configurations;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var enabled = configuration.GetValue("RateLimiting:Enabled", true);
        if (!enabled)
            return services;

        var globalLimit = configuration.GetValue("RateLimiting:Global:PermitLimit", 100);
        var globalWindowSeconds = configuration.GetValue("RateLimiting:Global:WindowSeconds", 60);
        var authLimit = configuration.GetValue("RateLimiting:Auth:PermitLimit", 10);
        var authWindowSeconds = configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60);
        var userApiLimit = configuration.GetValue("RateLimiting:UserApi:PermitLimit", 30);
        var userApiWindowSeconds = configuration.GetValue("RateLimiting:UserApi:WindowSeconds", 60);
        var passwordResetLimit = configuration.GetValue("RateLimiting:PasswordReset:PermitLimit", 5);
        var passwordResetWindowSeconds = configuration.GetValue("RateLimiting:PasswordReset:WindowSeconds", 3600);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // OnRejected: JSON response + Retry-After header
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;

                int? retryAfterSeconds = null;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfterSeconds = (int)retryAfter.TotalSeconds;
                    context.HttpContext.Response.Headers.RetryAfter =
                        retryAfterSeconds.Value.ToString(NumberFormatInfo.InvariantInfo);
                }

                var response = new RateLimitResponse(
                    "Too many requests. Please try again later.",
                    retryAfterSeconds);

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(response),
                    cancellationToken);
            };

            // Katman 1: Global IP-based limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = globalLimit,
                    Window = TimeSpan.FromSeconds(globalWindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });

            // Katman 2: Auth policy (IP-based, strict)
            options.AddPolicy("auth", httpContext =>
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter($"auth_{remoteIp}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authLimit,
                    Window = TimeSpan.FromSeconds(authWindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });

            // Katman 2b: Password-reset policy (IP-based, sliding window, very strict)
            options.AddPolicy("password-reset", httpContext =>
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter($"pwreset_{remoteIp}", _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = passwordResetLimit,
                    Window = TimeSpan.FromSeconds(passwordResetWindowSeconds),
                    SegmentsPerWindow = 6,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });

            // Katman 3: User-API policy (UserId for authenticated, IP for anonymous)
            options.AddPolicy("user-api", httpContext =>
            {
                var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var partitionKey = userId is not null
                    ? $"user_{userId}"
                    : $"anon_{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = userApiLimit,
                    Window = TimeSpan.FromSeconds(userApiWindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });

        return services;
    }
}
