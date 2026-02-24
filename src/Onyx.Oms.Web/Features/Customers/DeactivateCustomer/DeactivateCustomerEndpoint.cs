using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.DeactivateCustomer;

public class DeactivateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/customers")
            .WithApiVersionSet(app.NewApiVersionSet("Customers").Build()) 
            .HasApiVersion(1);

        group.MapPut("{id:guid}/deactivate", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new DeactivateCustomerCommand(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("Customers")
        .WithName("DeactivateCustomer")
        .WithSummary("Deactivate a customer")
        .WithDescription("Deactivates a customer.")
        .HasPermission(Permissions.Customers.Deactivate);
    }
}
