using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateProductSpecifications
{
    public class UpdateProductSpecificationsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/specifications", async (Guid id, ISender sender, UpdateProductSpecificationsCommand command) =>
            {
                if (id != command.Id)
                {
                    command = command with { Id = id };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateProductSpecifications")
            .WithSummary("Update product specifications")
            .WithDescription("Updates a product's specifications based on its category.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
