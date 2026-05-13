using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.GetOrderInvoice
{
    public class GetOrderInvoiceEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapGet("{id:guid}/invoice", async (ISender sender, [FromRoute] Guid id, [FromQuery] string logoStoragePath) =>
            {
                var query = new GetOrderInvoiceQuery(id, logoStoragePath);
                Result<byte[]> result = await sender.Send(query);
                if (result.IsFailure)
                {
                    return result.ToMinimalApiResult();
                }

                return Results.File(
                    fileContents: result.Value,
                    contentType: "application/pdf",
                    fileDownloadName: $"Invoice_{id}.pdf");
            })
                .WithTags("Orders")
                .WithName("GenerateOrderInvoice")
                .WithSummary("Generate a PDF Order Invoice")
                .WithDescription("Generates a PDF Invoice for a specific order.")
                .Produces<FileContentResult>(200, "application/pdf")
                .HasPermission(Permissions.Orders.View);
        }
    }
}
