using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Couriers.DeleteCourier;

public class DeleteCourierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) 
            .HasApiVersion(1);

        group.MapDelete("{id:guid}", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new DeleteCourierCommand(id));

            if (result.IsSuccess)
            {
                return Results.NoContent();
            }

            return result.ToProblemDetails();
        })
        .WithTags("Couriers")
        .WithName("DeleteCourier")
        .WithSummary("Delete a courier")
        .WithDescription("Deletes an existing courier.");
    }
}
