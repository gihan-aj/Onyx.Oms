using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GetProductsPaged;

public record GetProductsPagedQuery : PagedRequest, IQuery<PagedResult<ProductDto>>
{
    public Guid? CategoryId { get; init; }
    public bool? IsActive { get; init; }
}
