using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.CreateProductCategory;

public class CreateProductCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapPost("", async (ISender sender, [FromBody] CreateProductCategoryCommand command) =>
        {
            Result<Guid> result = await sender.Send(command);

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("CreateProductCategory")
        .WithSummary("Create a new product category")
        .WithDescription("Creates a new product category. Supports hierarchy.")
        .HasPermission(Permissions.ProductCategories.Create);
    }
}
