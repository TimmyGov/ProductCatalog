namespace ProductCatalog.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
