using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.DeleteProductCategory;

public class DeleteProductCategoryHandler : ICommandHandler<DeleteProductCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("ProductCategory.NotFound", "Category not found."));
        }

        // Business Rule: Cannot delete if it has sub-categories
        if (category.SubCategories.Any())
        {
            return Result.Failure(Error.Conflict("ProductCategory.HasChildren", "Cannot delete a category that has sub-categories. Please delete or move them first."));
        }

        // TODO: Check for Products once Products feature is implemented
        // if (await _context.Products.AnyAsync(p => p.CategoryId == category.Id)) ...

        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
