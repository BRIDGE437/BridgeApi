namespace BridgeApi.Application.Abstractions.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}
