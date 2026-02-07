using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces;
using System.Text;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<CategoryTreeDto>>> GetCategoryTree()
    {
        var categoryTree = await _categoryService.GetCategoryTreeAsync();
        return Ok(categoryTree);
    }

    // Manual model binding demonstration using raw request body
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory()
    {
        // Manual model binding: read request body manually
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest(new { error = "Request body is required" });
        }

        CreateCategoryDto? createCategoryDto;
        try
        {
            createCategoryDto = System.Text.Json.JsonSerializer.Deserialize<CreateCategoryDto>(
                body,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (System.Text.Json.JsonException ex)
        {
            return BadRequest(new { error = $"Invalid JSON: {ex.Message}" });
        }

        if (createCategoryDto is null)
        {
            return BadRequest(new { error = "Failed to parse category data" });
        }

        // Pattern matching for validation
        var validationError = createCategoryDto switch
        {
            { Name: null or "" } => "Category name is required",
            { Name.Length: > 100 } => "Category name cannot exceed 100 characters",
            { Description: null or "" } => "Category description is required",
            { ParentCategoryId: < 0 } => "Parent category ID must be positive or null",
            _ => null
        };

        if (validationError is not null)
        {
            return BadRequest(new { error = validationError });
        }

        var category = await _categoryService.CreateAsync(createCategoryDto);
        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }
}
