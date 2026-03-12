using BridgeApi.Application.Abstractions.Caching;
using BridgeApi.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Application.Behaviours;

public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehaviour<TRequest, TResponse>> _logger;

    public CachingBehaviour(ICacheService cacheService, ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
            return await next();

        var cacheKey = cacheableQuery.CacheKey;

        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse is not null)
        {
            _logger.LogInformation("Cache HIT for {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogInformation("Cache MISS for {CacheKey}", cacheKey);

        var response = await next();

        if (response is not null)
        {
            var duration = cacheableQuery.CacheDuration ?? DefaultCacheDuration;
            await _cacheService.SetAsync(cacheKey, response, duration, cancellationToken);
        }

        return response;
    }
}
