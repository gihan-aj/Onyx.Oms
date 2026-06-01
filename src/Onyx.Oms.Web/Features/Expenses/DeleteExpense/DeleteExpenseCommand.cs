using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.DeleteExpense
{
    public record DeleteExpenseCommand(Guid ExpenseId) : ICommand;
}
