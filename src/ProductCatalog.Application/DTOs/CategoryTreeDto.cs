namespace ProductCatalog.Application.DTOs;

public record CategoryTreeDto(
    int Id,
    string Name,
    string Description,
    int? ParentCategoryId,
    List<CategoryTreeDto> Children
);
