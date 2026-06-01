using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Expenses.CreateExpense;

namespace Onyx.Oms.Web.Features.Expenses.UpdateExpense
{
    public record UpdateExpenseCommand(
        Guid Id,
        string Category,
        decimal Amount,
        string Currency,
        DateTimeOffset DateIncurred,
        string? Reference,
        string? Notes) : ICommand;
}