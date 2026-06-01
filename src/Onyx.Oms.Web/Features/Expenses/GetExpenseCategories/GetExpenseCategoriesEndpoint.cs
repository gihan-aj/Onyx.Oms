using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Expenses.GetExpenseCategories;

public class GetExpenseCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/expenses")
            .WithApiVersionSet(app.NewApiVersionSet("Expenses").Build())
            .HasApiVersion(1);

        group.MapGet("categories", async (ISender sender) =>
        {
            Result<IReadOnlyList<string>> result = await sender.Send(new GetExpenseCategoriesQuery());

            return result.ToMinimalApiResult();
        })
        .WithTags("Expenses")
        .WithName("GetExpenseCategories")
        .WithSummary("Get expense categories")
        .WithDescription("Returns all available expense categories: the built-in defaults merged with any custom categories already used by this tenant.")
        .HasPermission(Permissions.Expenses.View);
    }
}
