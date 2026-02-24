using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Couriers.GetCouriersById;

public class GetCourierByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/couriers")
            .WithApiVersionSet(app.NewApiVersionSet("Couriers").Build()) 
            .HasApiVersion(1);

        group.MapGet("{id:guid}", async (ISender sender, Guid id) =>
        {
            Result<CourierDto> result = await sender.Send(new GetCourierByIdQuery(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("Couriers")
        .WithName("GetCourierById")
        .WithSummary("Get courier details")
        .WithDescription("Retrieves details of a specific courier by ID.")
        .HasPermission(Permissions.Couriers.View);
    }
}
