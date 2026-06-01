using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.GetExpensesPaged;

public class GetExpensesPagedHandler : IQueryHandler<GetExpensesPagedQuery, PagedResult<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExpensesPagedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ExpenseDto>>> Handle(GetExpensesPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Expenses
            .AsNoTracking();

        // 1. Filtering

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(e => e.Category == request.Category);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(e => e.DateIncurred >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(e => e.DateIncurred <= request.DateTo.Value);
        }

        if (request.MinAmount.HasValue)
        {
            query = query.Where(e => e.Amount.Amount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            query = query.Where(e => e.Amount.Amount <= request.MaxAmount.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(e =>
                e.Category.Contains(request.SearchTerm) ||
                (e.Reference != null && e.Reference.Contains(request.SearchTerm)) ||
                (e.Notes != null && e.Notes.Contains(request.SearchTerm)));
        }

        // 2. Sorting
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projection
        var dtoQuery = query.Select(e => new ExpenseDto(
            e.Id,
            e.Category,
            e.Amount.Amount,
            e.Amount.Currency,
            e.DateIncurred,
            e.Reference,
            e.Notes,
            e.CreatedOnUtc));

        // 4. Pagination
        var pagedResult = await PagedResult<ExpenseDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedResult);
    }

    private static IQueryable<Core.Domain.Entities.Expense> ApplySorting(
        IQueryable<Core.Domain.Entities.Expense> query,
        string? sortColumn,
        string? sortOrder)
    {
        bool isDesc = sortOrder?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return query.OrderByDescending(e => e.DateIncurred); // Default: most recent first
        }

        return sortColumn.ToLower() switch
        {
            "category"     => isDesc ? query.OrderByDescending(e => e.Category)     : query.OrderBy(e => e.Category),
            "amount"       => isDesc ? query.OrderByDescending(e => e.Amount.Amount) : query.OrderBy(e => e.Amount.Amount),
            "dateincurred" => isDesc ? query.OrderByDescending(e => e.DateIncurred)  : query.OrderBy(e => e.DateIncurred),
            "reference"    => isDesc ? query.OrderByDescending(e => e.Reference)     : query.OrderBy(e => e.Reference),
            "createddate"  => isDesc ? query.OrderByDescending(e => e.CreatedOnUtc)  : query.OrderBy(e => e.CreatedOnUtc),
            _              => query.OrderByDescending(e => e.DateIncurred)
        };
    }
}
