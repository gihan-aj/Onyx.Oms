using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;
using Onyx.Oms.Web.Features.Couriers.GetCouriers;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriersPaged;

public class GetCouriersPagedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) 
            .HasApiVersion(1);

        group.MapGet("search", async (ISender sender, [AsParameters] GetCouriersPagedQuery query) =>
        {
            Result<PagedResult<CourierDto>> result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("Couriers")
        .WithName("GetCouriersPaged")
        .WithSummary("Search couriers")
        .WithDescription("Retrieves a paginated list of couriers with optional searching, sorting, and filtering.");
    }
}
