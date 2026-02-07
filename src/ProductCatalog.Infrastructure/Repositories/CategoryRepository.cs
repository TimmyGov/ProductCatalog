using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly Dictionary<int, Category> _categories;
    private readonly object _lock = new object();
    private int _nextId = 1;

    public CategoryRepository()
    {
        _categories = new Dictionary<int, Category>();
    }

    public Task<Category?> GetById(int id)
    {
        lock (_lock)
        {
            _categories.TryGetValue(id, out var category);
            return Task.FromResult(category);
        }
    }

    public Task<IEnumerable<Category>> GetAll()
    {
        lock (_lock)
        {
            var categories = _categories.Values.ToList();
            return Task.FromResult<IEnumerable<Category>>(categories);
        }
    }

    public Task<Category> Add(Category entity)
    {
        lock (_lock)
        {
            if (entity.Id == 0)
            {
                entity.Id = _nextId++;
            }
            else
            {
                if (_nextId <= entity.Id)
                {
                    _nextId = entity.Id + 1;
                }
            }

            _categories[entity.Id] = entity;
            return Task.FromResult(entity);
        }
    }

    public Task<Category> Update(Category entity)
    {
        lock (_lock)
        {
            if (_categories.ContainsKey(entity.Id))
            {
                _categories[entity.Id] = entity;
            }
            return Task.FromResult(entity);
        }
    }

    public Task Delete(int id)
    {
        lock (_lock)
        {
            _categories.Remove(id);
            return Task.CompletedTask;
        }
    }

    public Task<IEnumerable<Category>> GetRootCategories()
    {
        lock (_lock)
        {
            var rootCategories = _categories.Values
                .Where(c => c.ParentCategoryId == null)
                .ToList();
            return Task.FromResult<IEnumerable<Category>>(rootCategories);
        }
    }

    public Task<IEnumerable<Category>> GetChildCategories(int parentCategoryId)
    {
        lock (_lock)
        {
            var childCategories = _categories.Values
                .Where(c => c.ParentCategoryId == parentCategoryId)
                .ToList();
            return Task.FromResult<IEnumerable<Category>>(childCategories);
        }
    }
}
