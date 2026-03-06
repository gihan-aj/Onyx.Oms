using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GetProductsPaged;

public record GetProductsPagedQuery : PagedRequest, IQuery<PagedResult<ProductDto>>
{
    public bool? IsActive { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? HasVariants { get; init; }
}

public record ProductDto(
    Guid Id,
    string Name,
    string BaseSku,
    Guid CategoryId,
    string CategoryName,
    string CategoryPath,
    decimal BasePriceAmount,
    string BasePriceCurrency,
    string? MainImageUrl,
    bool HasVariants,
    int StockOnHand,
    int AvailableQuantity,
    bool IsActive,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset? LastModifiedOnUtc);
