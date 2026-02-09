using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.ActivateProductCategory;

public class ActivateProductCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}/activate", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new ActivateProductCategoryCommand(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("ActivateProductCategory")
        .WithSummary("Activate a product category")
        .WithDescription("Activates a product category. Does NOT recursively activate children.");
    }
}
