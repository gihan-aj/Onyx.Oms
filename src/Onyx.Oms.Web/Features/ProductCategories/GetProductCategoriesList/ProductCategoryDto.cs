namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoriesList;

public record ProductCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    int Level,
    string Path,
    string NamePath,
    string? IconUrl,
    string? Color,
    int DisplayOrder,
    bool IsActive);
