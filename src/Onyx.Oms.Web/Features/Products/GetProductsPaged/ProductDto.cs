using Onyx.Oms.Core.Domain.Enums;

namespace Onyx.Oms.Web.Features.Products.GetProductsPaged;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    string? Brand,
    string? Material,
    Gender Gender,
    decimal BasePrice,
    bool IsActive,
    string? MainImageUrl,
    List<string> Tags,
    int VariantCount,
    int TotalStockOnHand);
