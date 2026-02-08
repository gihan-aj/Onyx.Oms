using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriers;

public class GetCouriersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) 
            .HasApiVersion(1);

        group.MapGet("", async (ISender sender, [AsParameters] GetArgs args) =>
        {
            Result<IEnumerable<CourierDto>> result = await sender.Send(new GetCouriersQuery(args.IsActive));

            return result.ToMinimalApiResult();
        })
        .WithTags("Couriers")
        .WithName("GetCouriers")
        .WithSummary("Get a list of couriers")
        .WithDescription("Retrieves all couriers with optional filtering by active status.");
    }

    // Helper record for query parameters binding
    internal record GetArgs(bool? IsActive);
}
