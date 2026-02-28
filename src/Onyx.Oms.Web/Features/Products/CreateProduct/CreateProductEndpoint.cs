using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/products")
            .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
            .HasApiVersion(1);

        group.MapPost("", async (ISender sender, CreateProductCommand command) =>
        {
            var result = await sender.Send(command);
            return result.ToMinimalApiResult();
        })
        .WithTags("Products")
        .WithName("CreateProduct")
        .WithSummary("Creates a new product alongside optional variants and images.")
        .WithDescription("Creates a new product aggregate in one go. Validates currencies, units, and ensures uniqueness of generated SKUs.")
        .HasPermission(Permissions.Products.Create);
    }
}
