namespace ProductCatalog.Application.Interfaces;

public interface ISearchCacheService
{
    TValue? Get<TKey, TValue>(TKey key) where TKey : notnull;
    
    void Set<TKey, TValue>(TKey key, TValue value, TimeSpan? expiration = null) where TKey : notnull;
    
    void Clear();
}
