using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.ActivateCustomer;

public class ActivateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/customers")
            .WithApiVersionSet(app.NewApiVersionSet("Customers").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}/activate", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new ActivateCustomerCommand(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("Customers")
        .WithName("ActivateCustomer")
        .WithSummary("Activate a customer")
        .WithDescription("Activates a previously deactivated customer.");
    }
}
