using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.DeleteProductVariant
{
    public class DeleteProductVariantEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapDelete("{productId:guid}/variants/{variantId:guid}", async (Guid productId, Guid variantId, ISender sender) =>
            {
                var command = new DeleteProductVariantCommand(productId, variantId);

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("DeleteProductVariant")
            .WithSummary("Delete product variant")
            .WithDescription("Soft deletes a variant.")
            .HasPermission(Permissions.Products.Delete);
        }
    }
}
