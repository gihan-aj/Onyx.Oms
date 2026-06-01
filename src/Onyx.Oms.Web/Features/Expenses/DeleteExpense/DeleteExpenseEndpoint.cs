using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Expenses.DeleteExpense
{
    public class DeleteExpenseEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/expenses")
                .WithApiVersionSet(app.NewApiVersionSet("Expenses").Build())
                .HasApiVersion(1);
            group.MapDelete("{id:guid}", async (ISender sender, Guid id) =>
            {
                var command = new DeleteExpenseCommand(id);
                Result result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
            .WithTags("Expenses")
            .WithName("DeleteExpense")
            .WithSummary("Delete an existing expense")
            .WithDescription("Soft deletes an existing expense record.")
            .HasPermission(Permissions.Expenses.Delete);
        }
    }
}
