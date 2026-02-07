using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(
        int pageNumber = 1, 
        int pageSize = 10, 
        decimal? minPrice = null, 
        decimal? maxPrice = null,
        int? categoryId = null);
    
    Task<ProductDto?> GetByIdAsync(int id);
    
    Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
    
    Task<ProductDto> UpdateAsync(UpdateProductDto updateProductDto);
    
    Task<bool> DeleteAsync(int id);
    
    Task<IEnumerable<ProductDto>> SearchProductsAsync(
        string searchTerm, 
        int pageNumber = 1, 
        int pageSize = 10);
}
