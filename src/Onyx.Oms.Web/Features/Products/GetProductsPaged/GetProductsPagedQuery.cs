using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GetProductsPaged;

public record ProductDto(
    Guid Id,
    string Name,
    string BaseSku,
    string CategoryName,
    string? Brand,
    bool IsActive,
    bool HasColor,
    bool HasSize,
    decimal BasePriceAmount,
    string BasePriceCurrency,
    decimal BaseCostAmount,
    string BaseCostCurrency,
    int TotalStock,
    DateTimeOffset CreatedOnUtc);

public record GetProductsPagedQuery : PagedRequest, IQuery<PagedResult<ProductDto>>
{
    public bool? IsActive { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? HasColor { get; init; }
    public bool? HasSize { get; init; }
}
