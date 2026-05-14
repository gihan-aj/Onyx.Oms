using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderLogistics
{
    public class UpdateOrderLogisticsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPut("{id}/logistics", async (Guid id, [FromBody] UpdateOrderLogisticsRequest request, ISender sender) =>
            {
                var command = new UpdateOrderLogisticsCommand(
                    id, 
                    request.CourierId, 
                    request.ShippingAddress,
                    request.DeliveryInstructions);
                    
                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("UpdateOrderLogistics")
            .WithSummary("Update order logistics")
            .WithDescription("Updates the courier and shipping address for an order before it is shipped.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record UpdateOrderLogisticsRequest(
        Guid? CourierId,
        UpdateShippingAddressDto? ShippingAddress,
        string? DeliveryInstructions);
}
