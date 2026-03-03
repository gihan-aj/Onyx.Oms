using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryById;

public class GetProductCategoryByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapGet("{id:guid}", async (ISender sender, [FromRoute] Guid id, [FromQuery] bool includeParentSpecs = false) =>
        {
            var query = new GetProductCategoryByIdQuery(id, includeParentSpecs);
            Result<ProductCategoryResponse> result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("GetProductCategoryById")
        .WithSummary("Get a product category by ID")
        .WithDescription("Retrieves the details of a specific product category including its specifications.")
        .HasPermission(Permissions.ProductCategories.View);
    }
}
