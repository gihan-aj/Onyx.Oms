using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.CreateProductCategory;

public record CreateProductCategoryCommand(
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int DisplayOrder,
    string? IconUrl,
    string? Color,
    List<SpecDefinition>? Specifications) : ICommand<Guid>;
