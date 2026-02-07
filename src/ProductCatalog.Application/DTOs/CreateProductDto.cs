namespace ProductCatalog.Application.DTOs;

public record CreateProductDto(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId
);
