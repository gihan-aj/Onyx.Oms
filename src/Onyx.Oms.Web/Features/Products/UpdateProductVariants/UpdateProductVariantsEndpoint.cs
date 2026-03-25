using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariants
{
    public class UpdateProductVariantsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{productId:guid}/variants", async (Guid productId, Guid variantId, ISender sender, UpdateProductVariantsCommand command) =>
            {
                if (productId != command.ProductId)
                {
                    command = command with { ProductId = productId };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateProductVariants")
            .WithSummary("Update variant details")
            .WithDescription("Updates a variant's sku, specific price, cost, weight, and stock on hand and change status.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
