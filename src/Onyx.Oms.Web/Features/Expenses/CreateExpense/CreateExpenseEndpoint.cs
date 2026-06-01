using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Expenses.CreateExpense
{
    public class CreateExpenseEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/expenses")
                .WithApiVersionSet(app.NewApiVersionSet("Expenses").Build())
                .HasApiVersion(1);
            group.MapPost("", async (ISender sender, CreateExpenseCommand command) =>
            {
                Result<Guid> result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
            .WithTags("Expenses")
            .WithName("CreateExpense")
            .WithSummary("Create a new expense")
            .WithDescription("Creates a new expense record.")
            .HasPermission(Permissions.Expenses.Create);
        }
    }
}
