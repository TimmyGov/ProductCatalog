using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ProductCatalogDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategory(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return await GetAll();
        }

        return await _dbSet
            .Where(p => p.Name.Contains(name))
            .ToListAsync();
    }
}
