using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetSummary
{
    public class GetCatalogDashboardSummaryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/catalog/dashboard")
                .WithApiVersionSet(app.NewApiVersionSet("Catalog").Build())
                .HasApiVersion(1);
            group.MapGet("summary", async ([FromQuery] int lowStockThreshold, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetCatalogDashboardSummaryQuery(lowStockThreshold);
                Result<CatalogDashboardSummaryDto> result = await sender.Send(query, cancellationToken);
                return result.ToMinimalApiResult();
            })
            .WithTags("Catalog Dashboard")
            .WithName("GetDashboardSummary")
            .WithSummary("Get catalog dashboard summary")
            .WithDescription("Retrieves the statistics of the catalog for the dashboard.")
            .Produces<CatalogDashboardSummaryDto>()
            .HasPermission(Permissions.Products.View);
        }
    }
}
