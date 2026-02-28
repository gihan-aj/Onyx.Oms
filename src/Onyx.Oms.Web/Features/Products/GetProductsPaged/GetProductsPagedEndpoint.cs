using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
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

        group.MapGet("search", async (ISender sender, [AsParameters] GetProductsPagedQuery query) =>
        {
            Result<PagedResult<ProductDto>> result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("Products")
        .WithName("GetProductsPaged")
        .WithSummary("Search products")
        .WithDescription("Retrieves a paginated list of catalog products with optional searching, sorting, and filtering.")
        .HasPermission(Permissions.Products.View);
    }
}
