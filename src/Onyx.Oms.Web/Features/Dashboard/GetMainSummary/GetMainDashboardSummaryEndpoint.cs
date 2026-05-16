using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Dashboard.GetMainSummary
{
    public class GetMainDashboardSummaryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/dashboard")
                .WithApiVersionSet(app.NewApiVersionSet("Dashboard").Build())
                .HasApiVersion(1);

            group.MapGet("summary", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetMainDashboardSummaryQuery(), cancellationToken);
                return result.ToMinimalApiResult();
            })
            .WithTags("Dashboard")
            .WithName("GetMainDashboardSummary")
            .WithSummary("Get main dashboard summary")
            .Produces<MainDashboardSummaryDto>()
            .HasPermission(Permissions.Orders.View);
        }
    }
}
