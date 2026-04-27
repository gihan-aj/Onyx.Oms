using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderFinancials
{
    public class UpdateOrderFinancialsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPut("{id}/financials", async (Guid id, [FromBody] UpdateOrderFinancialsRequest request, ISender sender) =>
            {
                var command = new UpdateOrderFinancialsCommand(
                    id, 
                    request.Items, 
                    request.ShippingFee, 
                    request.TaxAmount, 
                    request.Discount);
                    
                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("UpdateOrderFinancials")
            .WithSummary("Update order items and financials")
            .WithDescription("Updates the order items, shipping fee, tax, and overall discount for a pending order.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record UpdateOrderFinancialsRequest(
        List<UpdateOrderItemDto> Items,
        UpdateMoneyDto? ShippingFee,
        UpdateMoneyDto? TaxAmount,
        UpdateOrderDiscountDto? Discount);
}
