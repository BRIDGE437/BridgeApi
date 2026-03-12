using System.Text.Json;
using BridgeApi.Application.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Infrastructure.Services.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await _cache.GetAsync(key, cancellationToken);
            if (bytes is null || bytes.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            var options = new DistributedCacheEntryOptions();

            if (expiration.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiration;

            await _cache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis REMOVE failed for key {CacheKey}", key);
        }
    }
}
