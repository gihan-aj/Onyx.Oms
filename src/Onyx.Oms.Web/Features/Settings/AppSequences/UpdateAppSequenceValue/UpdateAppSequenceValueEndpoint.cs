using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.AppSequences.UpdateAppSequenceValue;

public class UpdateAppSequenceValueEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v{version:apiVersion}/settings/sequences/{id}", async (string id, [FromBody] long newValue, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAppSequenceValueCommand(id.ToUpperInvariant(), newValue));
            return result.ToMinimalApiResult();
        })
        .WithTags("Settings")
        .WithSummary("Update current sequence value")
        .WithDescription("Updates the current value for a given sequence ID. The new value cannot be less than the existing current value.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .HasPermission(Permissions.AppSequences.Edit);
    }
}
