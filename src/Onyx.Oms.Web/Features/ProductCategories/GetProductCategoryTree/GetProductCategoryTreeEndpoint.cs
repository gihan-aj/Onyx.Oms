using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryTree;

public class GetProductCategoryTreeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapGet("tree", async (ISender sender, [FromQuery] bool? isActive = null) =>
        {
            Result<List<ProductCategoryTreeDto>> result = await sender.Send(new GetProductCategoryTreeQuery(isActive));

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("GetProductCategoryTree")
        .WithSummary("Get product category tree")
        .WithDescription("Retrieves the full hierarchy of product categories as a tree.");
    }
}
