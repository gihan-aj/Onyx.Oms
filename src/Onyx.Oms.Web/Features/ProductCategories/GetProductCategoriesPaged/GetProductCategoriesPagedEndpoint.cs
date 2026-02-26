using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoriesPaged;

public class GetProductCategoriesPagedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapGet("search", async (ISender sender, [AsParameters] GetProductCategoriesPagedQuery query) =>
        {
            Result<PagedResult<ProductCategoryDto>> result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("GetProductCategoriesPaged")
        .WithSummary("Search product categories")
        .WithDescription("Retrieves a paginated list of product categories with optional searching (Name, Description) and sorting.")
        .HasPermission(Permissions.ProductCategories.View);
    }
}
