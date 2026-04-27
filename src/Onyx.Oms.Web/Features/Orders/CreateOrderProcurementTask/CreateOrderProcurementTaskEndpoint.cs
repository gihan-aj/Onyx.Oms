using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.CreateOrderProcurementTask
{
    public class CreateOrderProcurementTaskEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPost("{orderId}/items/{orderItemId}/procurement-tasks", async (
                Guid orderId, 
                Guid orderItemId, 
                [FromBody] CreateOrderProcurementTaskRequest request, 
                ISender sender) =>
            {
                var command = new CreateOrderProcurementTaskCommand(
                    orderId, 
                    orderItemId, 
                    request.RequestedQuantity, 
                    request.Notes,
                    request.ExpectedCompletionDate,
                    request.Priority);
                    
                Result<Guid> result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("CreateOrderProcurementTask")
            .WithSummary("Create a procurement task for an order item")
            .WithDescription("Creates a new procurement task linked to the specified order item and updates the item status.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record CreateOrderProcurementTaskRequest(
        int RequestedQuantity,
        string? Notes,
        DateTimeOffset? ExpectedCompletionDate,
        TaskPriority Priority = TaskPriority.Normal);
}
