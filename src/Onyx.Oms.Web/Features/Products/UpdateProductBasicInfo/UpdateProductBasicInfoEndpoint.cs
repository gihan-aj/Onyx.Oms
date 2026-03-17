using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.UpdateProductBasicInfo
{
    public class UpdateProductBasicInfoEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/basic-info", async (Guid id, ISender sender, UpdateProductBasicInfoCommand command) =>
            {
                if (id != command.Id)
                {
                    command = command with { Id = id };
                }

                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("UpdateProductBasicInfo")
            .WithSummary("Update product basic information")
            .WithDescription("Updates a product's name, description, category, base SKU, and tags.")
            .HasPermission(Permissions.Products.Edit);
        }
    }
}
