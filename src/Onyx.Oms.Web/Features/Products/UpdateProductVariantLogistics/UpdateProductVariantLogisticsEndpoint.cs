using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariantLogistics
{
    public class UpdateProductVariantLogisticsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{productId:guid}/variants/{variantId:guid}/logistics", async (Guid productId, Guid variantId, ISender sender, UpdateProductVariantLogisticsCommand command) =>
            {
                if (productId != command.ProductId || variantId != command.VariantId)
                {
                    command = command with { ProductId = productId, VariantId = variantId };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateProductVariantLogistics")
            .WithSummary("Update variant logistics")
            .WithDescription("Updates a variant's specific price, cost, weight, and stock on hand.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
