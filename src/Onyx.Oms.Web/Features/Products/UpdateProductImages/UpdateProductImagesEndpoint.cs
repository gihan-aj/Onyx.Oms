using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateProductImages
{
    public class UpdateProductImagesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/images", async (Guid id, ISender sender, UpdateProductImagesCommand command) =>
            {
                if (id != command.ProductId)
                {
                    command = command with { ProductId = id };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateProductImages")
            .WithSummary("Update product images")
            .WithDescription("Updates a product's images, handling both additions, removals, and updates of existing images.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
