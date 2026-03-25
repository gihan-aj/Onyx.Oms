using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.ActivateProduct
{
    public class ActivateProductEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPatch("{productId:guid}/activate", async (Guid productId, ISender sender) =>
            {
                Result result = await sender.Send(new ActivateProductCommand(productId));
                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("ActivateProduct")
            .WithSummary("Activate a product")
            .WithDescription("Activates a base product.")
            .HasPermission(Permissions.Products.Activate);
        }
    }
}
