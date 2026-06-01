using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Expenses.UpdateExpense
{
    public class UpdateExpenseEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/expenses")
                .WithApiVersionSet(app.NewApiVersionSet("Expenses").Build())
                .HasApiVersion(1);
            group.MapPut("{id:guid}", async (ISender sender, Guid id, UpdateExpenseRequest request) =>
            {
                var command = new UpdateExpenseCommand(
                    Id: id,
                    Category: request.Category,
                    Amount: request.Amount,
                    Currency: request.Currency,
                    DateIncurred: request.DateIncurred,
                    Reference: request.Reference,
                    Notes: request.Notes
                );
                Result result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
            .WithTags("Expenses")
            .WithName("UpdateExpense")
            .WithSummary("Update an existing expense")
            .WithDescription("Updates an existing expense record.")
            .HasPermission(Permissions.Expenses.Edit);
        }
    }

    public record UpdateExpenseRequest(
        string Category,
        decimal Amount,
        string Currency,
        DateTimeOffset DateIncurred,
        string? Reference,
        string? Notes);
}