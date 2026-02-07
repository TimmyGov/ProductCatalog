using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    
    Task<CategoryDto?> GetByIdAsync(int id);
    
    Task<CategoryDto> CreateAsync(CreateCategoryDto createCategoryDto);
    
    Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync();
}
