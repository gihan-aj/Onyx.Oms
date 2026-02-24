using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.DeleteProductCategory;

public class DeleteProductCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapDelete("{id:guid}", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new DeleteProductCategoryCommand(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("DeleteProductCategory")
        .WithSummary("Delete a product category")
        .WithDescription("Deletes a product category if it has no children.")
        .HasPermission(Permissions.ProductCategories.Delete);
    }
}
