using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoriesList;

public class GetProductCategoriesListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapGet("", async (ISender sender, [FromQuery] bool onlyLeaves = false, [FromQuery] bool? isActive = null) =>
        {
            Result<List<ProductCategoryDto>> result = await sender.Send(new GetProductCategoriesListQuery(onlyLeaves, isActive));

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("GetProductCategories")
        .WithSummary("Get product categories list")
        .WithDescription("Retrieves a flat list of product categories, optionally filtered to only include leaf categories (those with no sub-categories).")
        .HasPermission(Permissions.ProductCategories.View);
    }
}
