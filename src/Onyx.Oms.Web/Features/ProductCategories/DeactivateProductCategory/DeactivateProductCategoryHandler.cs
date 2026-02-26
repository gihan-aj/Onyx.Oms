using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.DeactivateProductCategory;

public class DeactivateProductCategoryHandler : ICommandHandler<DeactivateProductCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateProductCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        // Load category with all descendants to ensure recursion works in memory
        var category = await _context.ProductCategories
            .Include(c => c.SubCategories)
            .ThenInclude(sc => sc.SubCategories) // MaxDepth=2, covering 3 levels: Root-Sub-SubSub
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("ProductCategory.NotFound", "Category not found."));
        }

        if (!category.IsActive)
            return Result.Success();

        category.Deactivate(); // This calls Deactivate() on children recursively
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
