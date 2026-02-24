using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.ProductCategories.UpdateProductCategory;

public class UpdateProductCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/product-categories")
            .WithApiVersionSet(app.NewApiVersionSet("ProductCategories").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}", async (ISender sender, Guid id, [FromBody] UpdateProductCategoryCommand command) =>
        {
            if (id != command.Id)
            {
                return Results.Problem(statusCode: 400, title: "Bad Request", detail: "ID in path does not match ID in body.");
            }

            Result result = await sender.Send(command);

            return result.ToMinimalApiResult();
        })
        .WithTags("ProductCategories")
        .WithName("UpdateProductCategory")
        .WithSummary("Update a product category")
        .WithDescription("Updates a product category. Handles moving categories (and their subtrees) to new parents.")
        .HasPermission(Permissions.ProductCategories.Edit);
    }
}
