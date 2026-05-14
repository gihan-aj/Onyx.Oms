using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.GenerateBulkShippingLabels
{
    public class GenerateBulkShippingLabelsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("bulk-shipping-labels", async (ISender sender, [FromBody] BulkShippingLabelRequest body) =>
            {
                var query = new GenerateBulkShippingLabelsQuery(body.OrderIds);
                Result<byte[]> result = await sender.Send(query);

                if (result.IsFailure)
                {
                    return result.ToMinimalApiResult();
                }

                return Results.File(
                    fileContents: result.Value,
                    contentType: "application/pdf",
                    fileDownloadName: $"Bulk_Shipping_Labels_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            })
                .WithTags("Orders")
                .WithName("GenerateBulkShippingLabels")
                .WithSummary("Generate bulk PDF shipping labels")
                .WithDescription("Generates a single PDF containing shipping labels for multiple orders.")
                .Produces<FileContentResult>(200, "application/pdf")
                .HasPermission(Permissions.Orders.View);

        }

        public record BulkShippingLabelRequest(List<Guid> OrderIds);
    }
}
