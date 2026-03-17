using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateDefaultVariantLogistics
{
    public class UpdateDefaultVariantLogisticsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/default-variant-logistics", async (Guid id, ISender sender, UpdateDefaultVariantLogisticsCommand command) =>
            {
                if (id != command.ProductId)
                {
                    command = command with { ProductId = id };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateDefaultVariantLogistics")
            .WithSummary("Update default variant logistics")
            .WithDescription("Updates the logistics for a variant-less product.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
