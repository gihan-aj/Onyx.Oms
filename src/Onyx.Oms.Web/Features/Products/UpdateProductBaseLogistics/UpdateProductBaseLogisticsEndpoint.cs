using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateProductBaseLogistics
{
    public class UpdateProductBaseLogisticsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/base-logistics", async (Guid id, ISender sender, UpdateProductBaseLogisticsCommand command) =>
            {
                if (id != command.Id)
                {
                    command = command with { Id = id };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateProductBaseLogistics")
            .WithSummary("Update product base logistics")
            .WithDescription("Updates a product's base price, base cost, and base weight.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
