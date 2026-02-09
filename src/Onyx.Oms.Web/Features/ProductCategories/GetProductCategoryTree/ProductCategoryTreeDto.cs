namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryTree;

public record ProductCategoryTreeDto(
    Guid Id,
    string Name,
    string? Description,
    int Level,
    string? IconUrl,
    string? Color,
    int DisplayOrder,
    bool IsActive,
    List<ProductCategoryTreeDto> SubCategories);
