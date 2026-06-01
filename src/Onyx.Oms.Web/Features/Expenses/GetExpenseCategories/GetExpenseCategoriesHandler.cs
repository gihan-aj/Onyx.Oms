using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.GetExpenseCategories;

public class GetExpenseCategoriesHandler : IQueryHandler<GetExpenseCategoriesQuery, IReadOnlyList<string>>
{
    private readonly IApplicationDbContext _context;

    public GetExpenseCategoriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<string>>> Handle(GetExpenseCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Fetch distinct categories already used by this tenant
        var usedCategories = await _context.Expenses
            .AsNoTracking()
            .Select(e => e.Category)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Merge with defaults, deduplicate, and sort alphabetically
        var allCategories = ExpenseCategories.Defaults
            .Union(usedCategories, StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();

        return Result.Success<IReadOnlyList<string>>(allCategories);
    }
}
