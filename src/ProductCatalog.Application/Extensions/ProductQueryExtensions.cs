using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Extensions;

public static class ProductQueryExtensions
{
    /// <summary>
    /// Filters products by price range
    /// </summary>
    /// <param name="query">The product queryable</param>
    /// <param name="minPrice">Minimum price (inclusive)</param>
    /// <param name="maxPrice">Maximum price (inclusive)</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<Product> FilterByPrice(
        this IQueryable<Product> query, 
        decimal? minPrice, 
        decimal? maxPrice)
    {
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }
        
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }
        
        return query;
    }
    
    /// <summary>
    /// Searches products by name (case-insensitive contains)
    /// </summary>
    /// <param name="query">The product queryable</param>
    /// <param name="searchTerm">Search term to match in product name</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<Product> SearchByName(
        this IQueryable<Product> query, 
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }
        
        return query.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));
    }
    
    /// <summary>
    /// Filters products by category
    /// </summary>
    /// <param name="query">The product queryable</param>
    /// <param name="categoryId">Category ID to filter by</param>
    /// <returns>Filtered queryable</returns>
    public static IQueryable<Product> FilterByCategory(
        this IQueryable<Product> query, 
        int? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return query;
        }
        
        return query.Where(p => p.CategoryId == categoryId.Value);
    }
    
    /// <summary>
    /// Sorts products by name
    /// </summary>
    /// <param name="query">The product queryable</param>
    /// <param name="ascending">True for ascending, false for descending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<Product> SortByName(
        this IQueryable<Product> query, 
        bool ascending = true)
    {
        return ascending 
            ? query.OrderBy(p => p.Name) 
            : query.OrderByDescending(p => p.Name);
    }
    
    /// <summary>
    /// Sorts products by price
    /// </summary>
    /// <param name="query">The product queryable</param>
    /// <param name="ascending">True for ascending, false for descending</param>
    /// <returns>Sorted queryable</returns>
    public static IQueryable<Product> SortByPrice(
        this IQueryable<Product> query, 
        bool ascending = true)
    {
        return ascending 
            ? query.OrderBy(p => p.Price) 
            : query.OrderByDescending(p => p.Price);
    }
}
