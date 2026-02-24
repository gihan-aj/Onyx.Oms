using Asp.Versioning;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Customers.DeleteCustomer;

public class DeleteCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/customers")
            .WithApiVersionSet(app.NewApiVersionSet("Customers").Build()) 
            .HasApiVersion(1);

        group.MapDelete("{id:guid}", async (ISender sender, Guid id) =>
        {
            Result result = await sender.Send(new DeleteCustomerCommand(id));

            return result.ToMinimalApiResult();
        })
        .WithTags("Customers")
        .WithName("DeleteCustomer")
        .WithSummary("Delete a customer")
        .WithDescription("Deletes a customer by their unique identifier.")
        .HasPermission(Permissions.Customers.Delete);
    }
}
