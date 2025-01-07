using Microsoft.Extensions.Caching.Hybrid;
public interface IHybridCacheWrapper
{
    ValueTask<TItem> GetOrCreateAsync<TItem>(string key, Func<CancellationToken, ValueTask<TItem>> factory, HybridCacheEntryOptions options = null, IEnumerable<string> context = null, CancellationToken token = default);
    ValueTask RemoveAsync(string key, CancellationToken token = default);
}