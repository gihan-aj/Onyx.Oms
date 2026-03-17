using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateProductOptions
{
    public class UpdateProductOptionsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/options", async (Guid id, ISender sender, UpdateProductOptionsCommand command) =>
            {
                if (id != command.Id)
                {
                    command = command with { Id = id };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateProductOptions")
            .WithSummary("Update product options")
            .WithDescription("Updates a product's dynamic options matrix.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
