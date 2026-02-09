using System.Text.Json.Serialization;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.UpdateProductCategory;

public record UpdateProductCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int DisplayOrder,
    string? IconUrl,
    string? Color) : ICommand;
