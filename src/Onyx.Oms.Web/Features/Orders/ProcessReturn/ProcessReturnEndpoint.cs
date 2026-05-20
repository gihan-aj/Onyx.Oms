using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.ProcessReturn
{
    public class ProcessReturnEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);
            group.MapPost("{id}/process-return", async (Guid id, [FromBody] ProcessReturnRequest request, ISender sender) =>
            {
                Result result = await sender.Send(new ProcessReturnCommand(id, request.ItemsToReturn, request.Reason));
                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("ProcessReturn")
            .WithSummary("Process returned items")
            .WithDescription("Adjusts inventory for returned items and marks the order as ReturnProcessed.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }
}
