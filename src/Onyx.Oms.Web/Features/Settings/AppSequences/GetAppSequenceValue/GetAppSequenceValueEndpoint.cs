using MediatR;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.AppSequences.GetAppSequenceValue;

public class GetAppSequenceValueEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/settings/sequences")
            .WithApiVersionSet(app.NewApiVersionSet("AppSequences").Build()) 
            .HasApiVersion(1);

        group.MapGet("{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetAppSequenceValueQuery(id.ToUpperInvariant()));
            return result.ToMinimalApiResult();
        })
        .WithTags("Settings")
        .WithName("GetAppSequence")
        .WithSummary("Get current sequence value")
        .WithDescription("Retrieves the current value for a given sequence ID (e.g., ORD, SKU).")
        .Produces<long>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .HasPermission(Permissions.AppSequences.View);
    }
}
