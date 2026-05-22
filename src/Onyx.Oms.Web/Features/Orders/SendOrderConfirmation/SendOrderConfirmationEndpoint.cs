using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.SendOrderConfirmation
{
    public class SendOrderConfirmationEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{id:guid}/send-confirmation", async (ISender sender, [FromRoute] Guid id) =>
            {
                var command = new SendOrderConfirmationCommand(id);
                Result<string> result = await sender.Send(command);

                // If it's an error (like RateLimitExceeded or BadGateway), ToMinimalApiResult handles the HTTP status codes perfectly!
                if (result.IsFailure)
                {
                    return result.ToMinimalApiResult();
                }

                // Return 200 OK with the Meta Message ID
                return Results.Ok(new { MessageId = result.Value, Status = "Sent" });
            })
            .WithTags("Orders")
            .WithName("SendOrderWhatsAppConfirmation")
            .WithSummary("Send WhatsApp Order Confirmation")
            .WithDescription("Triggers a WhatsApp template message to the customer confirming their order.")
            .Produces(200)
            .ProducesProblem(400) // Validation
            .ProducesProblem(429) // Rate Limited
            .ProducesProblem(502) // Meta API Error
            .HasPermission(Permissions.Orders.Edit);
        }
    }
}
