namespace ProductCatalog.Application.DTOs;

public record CreateCategoryDto(
    string Name,
    string Description,
    int? ParentCategoryId
);
