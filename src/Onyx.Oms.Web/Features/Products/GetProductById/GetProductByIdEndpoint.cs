using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.GetProductById;

public class GetProductByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/products")
            .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
            .HasApiVersion(1);

        group.MapGet("{id:guid}", async (ISender sender, [FromRoute] Guid id) =>
        {
            var query = new GetProductByIdQuery(id);
            Result<ProductDetailsDto> result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("Products")
        .WithName("GetProductById")
        .WithSummary("Get a product by ID")
        .WithDescription("Retrieves the details of a specific product.")
        .Produces<ProductDetailsDto>()
        .HasPermission(Permissions.Products.View);
    }
}
