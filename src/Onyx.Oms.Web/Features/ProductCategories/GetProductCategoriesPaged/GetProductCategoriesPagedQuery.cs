using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoriesPaged;

public record GetProductCategoriesPagedQuery : PagedRequest, IQuery<PagedResult<ProductCategoryDto>>
{
    public bool? IsActive { get; init; }
    public bool? IsValidParent { get; init; }
    public bool? IsLeafOnly { get; init; }
}
