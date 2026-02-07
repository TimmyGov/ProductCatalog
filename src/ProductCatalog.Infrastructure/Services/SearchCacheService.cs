using ProductCatalog.Application.Interfaces;

namespace ProductCatalog.Infrastructure.Services;

public class SearchCacheService : ISearchCacheService
{
    private readonly Dictionary<object, CacheEntry> _cache;
    private readonly object _lock = new object();
    private readonly TimeSpan _defaultTtl;

    public SearchCacheService(TimeSpan? defaultTtl = null)
    {
        _cache = new Dictionary<object, CacheEntry>();
        _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(5);
    }

    public TValue? Get<TKey, TValue>(TKey key) where TKey : notnull
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt > DateTime.UtcNow)
                {
                    return (TValue?)entry.Value;
                }
                else
                {
                    _cache.Remove(key);
                }
            }
            return default;
        }
    }

    public void Set<TKey, TValue>(TKey key, TValue value, TimeSpan? expiration = null) where TKey : notnull
    {
        lock (_lock)
        {
            var ttl = expiration ?? _defaultTtl;
            var entry = new CacheEntry
            {
                Value = value,
                ExpiresAt = DateTime.UtcNow.Add(ttl)
            };
            _cache[key] = entry;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
        }
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
