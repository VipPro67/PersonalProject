using Microsoft.Extensions.Caching.Hybrid;

public class HybridCacheWrapper : IHybridCacheWrapper
{
    private readonly HybridCache _cache;

    public HybridCacheWrapper(HybridCache cache)
    {
        _cache = cache;
    }

    public ValueTask<TItem> GetOrCreateAsync<TItem>(string key, Func<CancellationToken, ValueTask<TItem>> factory, HybridCacheEntryOptions options = null, IEnumerable<string> context = null, CancellationToken token = default)
    {
        return _cache.GetOrCreateAsync(key, factory, options, context, token);
    }

    public ValueTask RemoveAsync(string key, CancellationToken token = default)
    {
        return _cache.RemoveAsync(key, token);
    }
}