using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategory(int categoryId);
    Task<IEnumerable<Product>> SearchByName(string name);
}
