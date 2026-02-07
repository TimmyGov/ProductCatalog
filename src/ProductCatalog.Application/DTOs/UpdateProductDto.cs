namespace ProductCatalog.Application.DTOs;

public record UpdateProductDto(
    int Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId
);
