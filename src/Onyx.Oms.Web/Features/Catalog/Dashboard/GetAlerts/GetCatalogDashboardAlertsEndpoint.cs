using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetAlerts
{
    public class GetCatalogDashboardAlertsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/catalog/dashboard")
                .WithApiVersionSet(app.NewApiVersionSet("Catalog").Build())
                .HasApiVersion(1);
            group.MapGet("alerts", async ([FromQuery] int lowStockThreshold, [FromQuery] int? limit, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetCatalogDashboardAlertsQuery(lowStockThreshold, limit ?? 3);
                Result<CatalogDashboardAlertsDto> result = await sender.Send(query, cancellationToken);
                return result.ToMinimalApiResult();
            })
            .WithTags("Catalog Dashboard")
            .WithName("GetDashboardAlerts")
            .WithSummary("Get catalog dashboard alerts")
            .WithDescription("Retrieves out-of-stock and low-stock alerts for the dashboard.")
            .Produces<CatalogDashboardAlertsDto>()
            .HasPermission(Permissions.Products.View);
        }
    }
}
