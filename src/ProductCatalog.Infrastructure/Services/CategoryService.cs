using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAll();
        return categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.ParentCategoryId
        ));
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetById(id);
        if (category == null)
        {
            return null;
        }

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId
        );
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto)
    {
        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description,
            ParentCategoryId = createCategoryDto.ParentCategoryId
        };

        var createdCategory = await _categoryRepository.Add(category);

        return new CategoryDto(
            createdCategory.Id,
            createdCategory.Name,
            createdCategory.Description,
            createdCategory.ParentCategoryId
        );
    }

    public async Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync()
    {
        var allCategories = await _categoryRepository.GetAll();
        var rootCategories = await _categoryRepository.GetRootCategories();

        var categoryTree = new List<CategoryTreeDto>();
        foreach (var rootCategory in rootCategories)
        {
            var treeNode = BuildCategoryTree(rootCategory, allCategories);
            categoryTree.Add(treeNode);
        }

        return categoryTree;
    }

    private CategoryTreeDto BuildCategoryTree(Category category, IEnumerable<Category> allCategories)
    {
        var children = allCategories
            .Where(c => c.ParentCategoryId == category.Id)
            .ToList();

        var childrenTree = new List<CategoryTreeDto>();
        foreach (var child in children)
        {
            var childTree = BuildCategoryTree(child, allCategories);
            childrenTree.Add(childTree);
        }

        return new CategoryTreeDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            childrenTree
        );
    }
}
