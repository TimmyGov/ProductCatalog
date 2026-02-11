using Moq;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Infrastructure.Services;

namespace ProductCatalog.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _productService = new ProductService(_mockProductRepository.Object, _mockCategoryRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var products = GenerateTestProducts(25);
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.GetAll())
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Category> { category });

        // Act
        var result = await _productService.GetAllAsync(pageNumber: 2, pageSize: 10);

        // Assert
        var resultList = result.Items.ToList();
        Assert.Equal(10, resultList.Count);
        Assert.Equal(11, resultList.First().Id); // Second page starts at item 11
        Assert.Equal(20, resultList.Last().Id);  // Second page ends at item 20
    }

    [Fact]
    public async Task GetAllAsync_WithMinPriceFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Cheap Product", Price = 10, CategoryId = 1 },
            new() { Id = 2, Name = "Mid Product", Price = 50, CategoryId = 1 },
            new() { Id = 3, Name = "Expensive Product", Price = 100, CategoryId = 1 }
        };
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.GetAll())
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Category> { category });

        // Act
        var result = await _productService.GetAllAsync(pageNumber: 1, pageSize: 10, minPrice: 50);

        // Assert
        var resultList = result.Items.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, p => Assert.True(p.Price >= 50));
    }

    [Fact]
    public async Task GetAllAsync_WithMaxPriceFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Cheap Product", Price = 10, CategoryId = 1 },
            new() { Id = 2, Name = "Mid Product", Price = 50, CategoryId = 1 },
            new() { Id = 3, Name = "Expensive Product", Price = 100, CategoryId = 1 }
        };
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.GetAll())
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Category> { category });

        // Act
        var result = await _productService.GetAllAsync(pageNumber: 1, pageSize: 10, maxPrice: 50);

        // Assert
        var resultList = result.Items.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, p => Assert.True(p.Price <= 50));
    }

    [Fact]
    public async Task GetAllAsync_WithCategoryFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10, CategoryId = 1 },
            new() { Id = 2, Name = "Product 2", Price = 20, CategoryId = 2 },
            new() { Id = 3, Name = "Product 3", Price = 30, CategoryId = 1 }
        };
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", ParentCategoryId = null },
            new() { Id = 2, Name = "Clothing", ParentCategoryId = null }
        };

        _mockProductRepository.Setup(r => r.GetAll())
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(categories);

        // Act
        var result = await _productService.GetAllAsync(pageNumber: 1, pageSize: 10, categoryId: 1);

        // Assert
        var resultList = result.Items.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, p => Assert.Equal(1, p.CategoryId));
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleFilters_AppliesAllFilters()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10, CategoryId = 1 },
            new() { Id = 2, Name = "Product 2", Price = 50, CategoryId = 1 },
            new() { Id = 3, Name = "Product 3", Price = 100, CategoryId = 1 },
            new() { Id = 4, Name = "Product 4", Price = 75, CategoryId = 2 }
        };
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.GetAll())
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Category> { category });

        // Act
        var result = await _productService.GetAllAsync(
            pageNumber: 1, 
            pageSize: 10, 
            minPrice: 40, 
            maxPrice: 90, 
            categoryId: 1);

        // Assert
        var resultList = result.Items.ToList();
        Assert.Single(resultList);
        Assert.Equal(2, resultList[0].Id);
        Assert.Equal(50, resultList[0].Price);
        Assert.Equal(1, resultList[0].CategoryId);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            SKU = "TEST-001",
            Price = 99.99m,
            Quantity = 10,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow
        };
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(product);
        _mockCategoryRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(category);

        // Act
        var result = await _productService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("Electronics", result.CategoryName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetById(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidProduct_ReturnsCreatedProductDto()
    {
        // Arrange
        var createDto = new CreateProductDto(
            Name: "New Product",
            Description: "New Description",
            SKU: "NEW-001",
            Price: 49.99m,
            Quantity: 5,
            CategoryId: 1
        );

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Description = createDto.Description,
            SKU = createDto.SKU,
            Price = createDto.Price,
            Quantity = createDto.Quantity,
            CategoryId = createDto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.Add(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);
        _mockCategoryRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(category);

        // Act
        var result = await _productService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Product", result.Name);
        Assert.Equal("Electronics", result.CategoryName);
        _mockProductRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_ReturnsUpdatedProductDto()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old Description",
            SKU = "OLD-001",
            Price = 10,
            Quantity = 5,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var updateDto = new UpdateProductDto(
            Id: 1,
            Name: "Updated Name",
            Description: "Updated Description",
            SKU: "UPD-001",
            Price: 20,
            Quantity: 10,
            CategoryId: 1
        );

        var updatedProduct = new Product
        {
            Id = updateDto.Id,
            Name = updateDto.Name,
            Description = updateDto.Description,
            SKU = updateDto.SKU,
            Price = updateDto.Price,
            Quantity = updateDto.Quantity,
            CategoryId = updateDto.CategoryId,
            CreatedAt = existingProduct.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(existingProduct);
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()))
            .ReturnsAsync(updatedProduct);
        _mockCategoryRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(category);

        // Act
        var result = await _productService.UpdateAsync(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.NotNull(result.UpdatedAt);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ThrowsException()
    {
        // Arrange
        var updateDto = new UpdateProductDto(
            Id: 999,
            Name: "Updated Name",
            Description: "Updated Description",
            SKU: "UPD-001",
            Price: 20,
            Quantity: 10,
            CategoryId: 1
        );

        _mockProductRepository.Setup(r => r.GetById(999))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _productService.UpdateAsync(updateDto));
        
        Assert.Contains("Product with ID 999 not found", exception.Message);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_ReturnsTrue()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 10,
            CategoryId = 1
        };

        _mockProductRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.Delete(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockProductRepository.Verify(r => r.Delete(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProduct_ReturnsFalse()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetById(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _productService.DeleteAsync(999);

        // Assert
        Assert.False(result);
        _mockProductRepository.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SearchProductsAsync_WithSearchTerm_ReturnsMatchingProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Gaming Laptop", Price = 1000, CategoryId = 1 },
            new() { Id = 2, Name = "Gaming Mouse", Price = 50, CategoryId = 1 },
            new() { Id = 3, Name = "Office Laptop", Price = 800, CategoryId = 1 }
        };
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.SearchByName("Gaming"))
            .ReturnsAsync(products.Where(p => p.Name.Contains("Gaming")));
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Category> { category });

        // Act
        var result = await _productService.SearchProductsAsync("Gaming", pageNumber: 1, pageSize: 10);

        // Assert
        var resultList = result.Items.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, p => Assert.Contains("Gaming", p.Name));
    }

    [Fact]
    public async Task SearchProductsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var products = GenerateTestProducts(25, namePrefix: "Gaming");
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.SearchByName("Gaming"))
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Category> { category });

        // Act
        var result = await _productService.SearchProductsAsync("Gaming", pageNumber: 2, pageSize: 10);

        // Assert
        var resultList = result.Items.ToList();
        Assert.Equal(10, resultList.Count);
        Assert.Equal(11, resultList.First().Id);
        Assert.Equal(20, resultList.Last().Id);
    }

    [Fact]
    public async Task SearchProductsAsync_NoMatches_ReturnsEmptyCollection()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Electronics", ParentCategoryId = null };

        _mockProductRepository.Setup(r => r.SearchByName("NonExistent"))
            .ReturnsAsync(Enumerable.Empty<Product>());
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(new List<Category> { category });

        // Act
        var result = await _productService.SearchProductsAsync("NonExistent", pageNumber: 1, pageSize: 10);

        // Assert
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAllAsync_IncludesCategoryName_WhenCategoryExists()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10, CategoryId = 1 }
        };
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", ParentCategoryId = null }
        };

        _mockProductRepository.Setup(r => r.GetAll())
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(categories);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        var productDto = result.Items.First();
        Assert.Equal("Electronics", productDto.CategoryName);
    }

    [Fact]
    public async Task GetAllAsync_CategoryNameIsNull_WhenCategoryNotFound()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10, CategoryId = 999 }
        };
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", ParentCategoryId = null }
        };

        _mockProductRepository.Setup(r => r.GetAll())
            .ReturnsAsync(products);
        _mockCategoryRepository.Setup(r => r.GetAll())
            .ReturnsAsync(categories);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        var productDto = result.Items.First();
        Assert.Null(productDto.CategoryName);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAtTimestamp()
    {
        // Arrange
        var createDto = new CreateProductDto(
            Name: "New Product",
            Description: "Description",
            SKU: "SKU-001",
            Price: 10,
            Quantity: 5,
            CategoryId: 1
        );

        Product? capturedProduct = null;
        _mockProductRepository.Setup(r => r.Add(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .ReturnsAsync((Product p) => { p.Id = 1; return p; });
        _mockCategoryRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(new Category { Id = 1, Name = "Test", ParentCategoryId = null });

        // Act
        var before = DateTime.UtcNow;
        await _productService.CreateAsync(createDto);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.True(capturedProduct.CreatedAt >= before && capturedProduct.CreatedAt <= after);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Name",
            Description = "Description",
            SKU = "SKU-001",
            Price = 10,
            Quantity = 5,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateDto = new UpdateProductDto(
            Id: 1,
            Name: "New Name",
            Description: "Description",
            SKU: "SKU-001",
            Price: 10,
            Quantity: 5,
            CategoryId: 1
        );

        Product? capturedProduct = null;
        _mockProductRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(existingProduct);
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .ReturnsAsync((Product p) => p);
        _mockCategoryRepository.Setup(r => r.GetById(1))
            .ReturnsAsync(new Category { Id = 1, Name = "Test", ParentCategoryId = null });

        // Act
        var before = DateTime.UtcNow;
        await _productService.UpdateAsync(updateDto);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(capturedProduct);
        Assert.NotNull(capturedProduct.UpdatedAt);
        Assert.True(capturedProduct.UpdatedAt >= before && capturedProduct.UpdatedAt <= after);
    }

    // Helper method to generate test products
    private List<Product> GenerateTestProducts(int count, string namePrefix = "Product")
    {
        var products = new List<Product>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(new Product
            {
                Id = i,
                Name = $"{namePrefix} {i}",
                Description = $"Description {i}",
                SKU = $"SKU-{i:D3}",
                Price = i * 10,
                Quantity = i,
                CategoryId = 1,
                CreatedAt = DateTime.UtcNow
            });
        }
        return products;
    }
}
