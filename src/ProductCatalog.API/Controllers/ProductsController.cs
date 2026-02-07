using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.Services.SearchEngine;
using ProductCatalog.Domain.Entities;
using System.Text.Json;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly SearchEngine<Product> _searchEngine;

    public ProductsController(IProductService productService, SearchEngine<Product> searchEngine)
    {
        _productService = productService;
        _searchEngine = searchEngine;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? searchTerm = null)
    {
        // Pattern matching for request validation
        var validationResult = (page, pageSize, minPrice, maxPrice) switch
        {
            ( <= 0, _, _, _) => "Page must be greater than 0",
            (_, <= 0, _, _) => "PageSize must be greater than 0",
            (_, > 100, _, _) => "PageSize cannot exceed 100",
            (_, _, < 0, _) => "MinPrice cannot be negative",
            (_, _, _, < 0) => "MaxPrice cannot be negative",
            (_, _, not null, not null) when minPrice > maxPrice => "MinPrice cannot be greater than MaxPrice",
            _ => null
        };

        if (validationResult is not null)
        {
            return BadRequest(new { error = validationResult });
        }

        IEnumerable<ProductDto> products;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            products = await _productService.SearchProductsAsync(searchTerm, page, pageSize);
        }
        else
        {
            products = await _productService.GetAllAsync(page, pageSize, minPrice, maxPrice, categoryId);
        }

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound(new { error = $"Product with ID {id} not found" });
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        // Pattern matching for product validation
        var validationError = createProductDto switch
        {
            { Name: null or "" } => "Product name is required",
            { Name.Length: > 200 } => "Product name cannot exceed 200 characters",
            { SKU: null or "" } => "SKU is required",
            { Price: < 0 } => "Price cannot be negative",
            { Quantity: < 0 } => "Quantity cannot be negative",
            { CategoryId: <= 0 } => "Valid category ID is required",
            _ => null
        };

        if (validationError is not null)
        {
            return BadRequest(new { error = validationError });
        }

        var product = await _productService.CreateAsync(createProductDto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        if (id != updateProductDto.Id)
        {
            return BadRequest(new { error = "ID mismatch between route and body" });
        }

        var existingProduct = await _productService.GetByIdAsync(id);
        if (existingProduct is null)
        {
            return NotFound(new { error = $"Product with ID {id} not found" });
        }

        var product = await _productService.UpdateAsync(updateProductDto);
        return Ok(product);
    }

    // Manual model binding demonstration
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // Manual model binding: read from query parameters manually
        var confirmHeader = Request.Headers["X-Confirm-Delete"].FirstOrDefault();
        var confirm = confirmHeader?.ToLowerInvariant() == "true";

        if (!confirm)
        {
            return BadRequest(new { error = "Delete operation requires X-Confirm-Delete: true header" });
        }

        var result = await _productService.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new { error = $"Product with ID {id} not found" });
        }

        return NoContent();
    }
}
