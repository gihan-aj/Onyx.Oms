using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Products.GenerateProductSheet
{
    public class GenerateProductSheetEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/products")
                .WithApiVersionSet(app.NewApiVersionSet("Products").Build())
                .HasApiVersion(1);

            group.MapGet("{id:guid}/sheet", async (ISender sender, [FromRoute] Guid id, [FromQuery] string imageStoragePath) =>
            {
                var query = new GenerateProductSheetQuery(id, imageStoragePath);
                Result<byte[]> result = await sender.Send(query);
                if(result.IsFailure)
                {
                    return result.ToMinimalApiResult();
                }

                return Results.File(
                    fileContents: result.Value,
                    contentType: "application/pdf",
                    fileDownloadName: $"ProductSheet_{id}.pdf");
            })
                .WithTags("Products")
                .WithName("GenerateProductSheet")
                .WithSummary("Generate a PDF Product Sheet")
                .WithDescription("Generates a PDF specification sheet for a specific product.")
                .Produces<FileContentResult>(200, "application/pdf")
                .HasPermission(Permissions.Products.View);
        }
    }
}
