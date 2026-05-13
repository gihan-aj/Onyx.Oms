using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.GenerateShippingLabel
{
    public class GenerateShippingLabelEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapGet("{id:guid}/shipping-label", async (ISender sender, [FromRoute] Guid id) =>
            {
                var query = new GenerateShippingLabelQuery(id);
                Result<byte[]> result = await sender.Send(query);
                if (result.IsFailure)
                {
                    return result.ToMinimalApiResult();
                }

                return Results.File(
                    fileContents: result.Value,
                    contentType: "application/pdf",
                    fileDownloadName: $"Shipping_Label_{id}.pdf");
            })
                .WithTags("Orders")
                .WithName("GenerateShippingLabel")
                .WithSummary("Generate a PDF shipping label")
                .WithDescription("Generates a PDF shipping label for a specific order.")
                .Produces<FileContentResult>(200, "application/pdf")
                .HasPermission(Permissions.Orders.View);
        }
    }
}
