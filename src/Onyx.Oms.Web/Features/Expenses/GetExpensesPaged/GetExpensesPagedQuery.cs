using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.GetExpensesPaged;

public record GetExpensesPagedQuery : PagedRequest, IQuery<PagedResult<ExpenseDto>>
{
    // Date range filter
    public DateTimeOffset? DateFrom { get; init; }
    public DateTimeOffset? DateTo { get; init; }

    // Category filter
    public string? Category { get; init; }

    // Amount range filter
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
}

public record ExpenseDto(
    Guid Id,
    string Category,
    decimal Amount,
    string Currency,
    DateTimeOffset DateIncurred,
    string? Reference,
    string? Notes,
    DateTimeOffset CreatedOnUtc);
