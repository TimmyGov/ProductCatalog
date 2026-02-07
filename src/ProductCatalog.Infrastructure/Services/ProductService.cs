using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? categoryId = null)
    {
        var products = await _productRepository.GetAll();

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            products = products.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= maxPrice.Value);
        }

        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var categoryIds = pagedProducts.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetAll();
        var categoryDict = categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c);

        var productDtos = pagedProducts.Select(product =>
        {
            categoryDict.TryGetValue(product.CategoryId, out var category);
            return new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.SKU,
                product.Price,
                product.Quantity,
                product.CategoryId,
                category?.Name,
                product.CreatedAt,
                product.UpdatedAt
            );
        }).ToList();

        return productDtos;
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetById(id);
        if (product == null)
        {
            return null;
        }

        var category = await _categoryRepository.GetById(product.CategoryId);
        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.SKU,
            product.Price,
            product.Quantity,
            product.CategoryId,
            category?.Name,
            product.CreatedAt,
            product.UpdatedAt
        );
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
    {
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            SKU = createProductDto.SKU,
            Price = createProductDto.Price,
            Quantity = createProductDto.Quantity,
            CategoryId = createProductDto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        var createdProduct = await _productRepository.Add(product);
        var category = await _categoryRepository.GetById(createdProduct.CategoryId);

        return new ProductDto(
            createdProduct.Id,
            createdProduct.Name,
            createdProduct.Description,
            createdProduct.SKU,
            createdProduct.Price,
            createdProduct.Quantity,
            createdProduct.CategoryId,
            category?.Name,
            createdProduct.CreatedAt,
            createdProduct.UpdatedAt
        );
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto updateProductDto)
    {
        var existingProduct = await _productRepository.GetById(updateProductDto.Id);
        if (existingProduct == null)
        {
            throw new InvalidOperationException($"Product with ID {updateProductDto.Id} not found.");
        }

        existingProduct.Name = updateProductDto.Name;
        existingProduct.Description = updateProductDto.Description;
        existingProduct.SKU = updateProductDto.SKU;
        existingProduct.Price = updateProductDto.Price;
        existingProduct.Quantity = updateProductDto.Quantity;
        existingProduct.CategoryId = updateProductDto.CategoryId;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _productRepository.Update(existingProduct);
        var category = await _categoryRepository.GetById(updatedProduct.CategoryId);

        return new ProductDto(
            updatedProduct.Id,
            updatedProduct.Name,
            updatedProduct.Description,
            updatedProduct.SKU,
            updatedProduct.Price,
            updatedProduct.Quantity,
            updatedProduct.CategoryId,
            category?.Name,
            updatedProduct.CreatedAt,
            updatedProduct.UpdatedAt
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepository.GetById(id);
        if (product == null)
        {
            return false;
        }

        await _productRepository.Delete(id);
        return true;
    }

    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(
        string searchTerm,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var products = await _productRepository.SearchByName(searchTerm);

        var pagedProducts = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var categoryIds = pagedProducts.Select(p => p.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetAll();
        var categoryDict = categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionary(c => c.Id, c => c);

        var productDtos = pagedProducts.Select(product =>
        {
            categoryDict.TryGetValue(product.CategoryId, out var category);
            return new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.SKU,
                product.Price,
                product.Quantity,
                product.CategoryId,
                category?.Name,
                product.CreatedAt,
                product.UpdatedAt
            );
        }).ToList();

        return productDtos;
    }
}
