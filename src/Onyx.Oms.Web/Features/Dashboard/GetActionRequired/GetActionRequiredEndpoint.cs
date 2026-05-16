using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Dashboard.GetActionRequired
{
    public class GetActionRequiredEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/dashboard")
                .WithApiVersionSet(app.NewApiVersionSet("Dashboard").Build())
                .HasApiVersion(1);

            group.MapGet("action-required", async ([FromQuery] int? limit, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetActionRequiredQuery(limit ?? 5), cancellationToken);
                return result.ToMinimalApiResult();
            })
            .WithTags("Dashboard")
            .WithName("GetActionRequired")
            .Produces<ActionRequiredListDto>()
            .HasPermission(Permissions.Orders.View);
        }
    }
}
