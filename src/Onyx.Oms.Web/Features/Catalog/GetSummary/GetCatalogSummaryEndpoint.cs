using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Catalog.GetSummary
{
    public class GetCatalogSummaryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/catalog")
                .WithApiVersionSet(app.NewApiVersionSet("Catalog").Build())
                .HasApiVersion(1);

            group.MapGet("summary", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetCatalogSummaryQuery();
                Result<CatalogSummaryDto> result = await sender.Send(query, cancellationToken);

                return result.ToMinimalApiResult();
            })
            .WithTags("Catalog")
            .WithName("GetCatalogSummary")
            .WithSummary("Get catalog summary")
            .WithDescription("Retrieves the statistics of the catalog.")
            .Produces<CatalogSummaryDto>()
            .HasPermission(Permissions.Products.View);
        }
    }
}
