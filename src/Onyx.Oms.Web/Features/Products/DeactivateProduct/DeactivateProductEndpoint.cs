using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.DeactivateProduct
{
    public class DeactivateProductEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPatch("{productId:guid}/deactivate", async (Guid productId, ISender sender) =>
            {
                Result result = await sender.Send(new DeactivateProductCommand(productId));
                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("DeactivateProduct")
            .WithSummary("Deactivate a product")
            .WithDescription("Deactivates a product and all its variants.")
            .HasPermission(Permissions.Products.Deactivate);
        }
    }
}
