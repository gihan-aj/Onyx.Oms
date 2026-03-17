using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.AddProductVariant
{
    public class AddProductVariantEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPost("{id:guid}/variants", async (Guid id, ISender sender, AddProductVariantCommand command) =>
            {
                if (id != command.ProductId)
                {
                    command = command with { ProductId = id };
                }

                Result<Guid> result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("AddProductVariant")
            .WithSummary("Add product variant")
            .WithDescription("Adds a new variant to an existing product.")
            .HasPermission(Permissions.Products.Create);
        }
    }
}
