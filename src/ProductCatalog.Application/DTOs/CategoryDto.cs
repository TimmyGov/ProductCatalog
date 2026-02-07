namespace ProductCatalog.Application.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string Description,
    int? ParentCategoryId
);
