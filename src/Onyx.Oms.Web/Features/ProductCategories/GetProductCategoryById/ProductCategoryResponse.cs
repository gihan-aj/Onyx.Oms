using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryById;

public record ProductCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int Level,
    string Path,
    string NamePath,
    bool IsActive,
    int DisplayOrder,
    string? IconUrl,
    string? Color,
    List<SpecDefinition> Specifications,
    List<SpecDefinition> AllSpecifications
);
