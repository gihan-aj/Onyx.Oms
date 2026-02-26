using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.ActivateProductCategory;

public class ActivateProductCategoryHandler : ICommandHandler<ActivateProductCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public ActivateProductCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("ProductCategory.NotFound", "Category not found."));
        }

        if(category.ParentCategory != null && !category.ParentCategory.IsActive)
        {
            return Result.Failure(Error.Validation("ProductCategory.ParentInactive", "Cannot activate category while its parent is inactive."));
        }

        if (category.IsActive)
            return Result.Success();

        category.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
