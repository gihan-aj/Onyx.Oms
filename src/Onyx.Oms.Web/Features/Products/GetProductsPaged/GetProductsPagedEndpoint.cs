using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.GetProductsPaged;

public class GetProductsPagedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/products")
            .WithApiVersionSet(app.NewApiVersionSet("Products").Build()) 
            .HasApiVersion(1);

        group.MapGet("", async (ISender sender, [AsParameters] GetProductsPagedQuery query) =>
        {
            var result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("Products")
        .WithName("GetProductsPaged")
        .WithSummary("Get products (paged)")
        .WithDescription("Retrieves a paged list of products. Supports search by name/sku/brand and filtering by category.")
        .HasPermission(Permissions.ProductCategories.View);
    }
}
