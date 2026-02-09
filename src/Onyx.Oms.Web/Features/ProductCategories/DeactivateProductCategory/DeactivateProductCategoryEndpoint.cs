using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.DeactivateProductCategory;

public class DeactivateProductCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}/deactivate", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new DeactivateProductCategoryCommand(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("DeactivateProductCategory")
        .WithSummary("Deactivate a product category")
        .WithDescription("Deactivates a product category and recursively deactivates all its sub-categories.");
    }
}
