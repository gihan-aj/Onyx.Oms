using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.CreateProduct
{
    public class CreateProductEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapPost("", async (ISender sender, CreateProductCommand command) =>
            {
                Result<Guid> result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Products")
            .WithName("CreateProducty")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product with variants and images.")
            .HasPermission(Permissions.Products.Create);
        }
    }
}
