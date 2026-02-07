using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategories();
    Task<IEnumerable<Category>> GetChildCategories(int parentCategoryId);
}
