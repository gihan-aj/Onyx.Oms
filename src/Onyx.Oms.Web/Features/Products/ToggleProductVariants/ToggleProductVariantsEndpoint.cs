using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.ToggleProductVariants
{
    public class ToggleProductVariantsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/toggle-variants", async (Guid id, ISender sender, ToggleProductVariantsCommand command) =>
            {
                if (id != command.Id)
                {
                    command = command with { Id = id };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("ToggleProductVariants")
            .WithSummary("Toggle product variants mode")
            .WithDescription("Turns the HasVariants flag on/off and automatically handles background variant data reconciliation.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
