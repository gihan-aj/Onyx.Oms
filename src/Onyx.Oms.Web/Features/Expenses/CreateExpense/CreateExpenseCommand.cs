using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.CreateExpense
{
    public record CreateExpenseCommand(
        string Category,
        decimal Amount,
        string Currency,
        DateTimeOffset DateIncurred,
        string? Reference,
        string? Notes) : ICommand<Guid>;
}
