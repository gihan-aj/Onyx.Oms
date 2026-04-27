using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderNotes
{
    public class UpdateOrderNotesEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
                .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
                .HasApiVersion(1);

            group.MapPut("{id}/notes", async (Guid id, [FromBody] UpdateOrderNotesRequest request, ISender sender) =>
            {
                var command = new UpdateOrderNotesCommand(
                    id, 
                    request.Notes);
                    
                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
            .WithTags("Orders")
            .WithName("UpdateOrderNotes")
            .WithSummary("Update order notes")
            .WithDescription("Updates the internal notes for an order.")
            .HasPermission(Permissions.Orders.Edit);
        }
    }

    public record UpdateOrderNotesRequest(string? Notes);
}
