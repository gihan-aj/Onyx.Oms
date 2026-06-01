using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Expenses.GetExpensesPaged;

public class GetExpensesPagedEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/expenses")
            .WithApiVersionSet(app.NewApiVersionSet("Expenses").Build())
            .HasApiVersion(1);

        group.MapGet("", async (ISender sender, [AsParameters] GetExpensesPagedQuery query) =>
        {
            Result<PagedResult<ExpenseDto>> result = await sender.Send(query);

            return result.ToMinimalApiResult();
        })
        .WithTags("Expenses")
        .WithName("GetExpensesPaged")
        .WithSummary("Search expenses")
        .WithDescription("Retrieves a paginated list of expenses with optional searching, sorting, date range, category, and amount filtering.")
        .HasPermission(Permissions.Expenses.View);
    }
}
