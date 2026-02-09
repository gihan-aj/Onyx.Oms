using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.ProductCategories.UpdateProductCategory;

public class UpdateProductCategoryHandler : IRequestHandler<UpdateProductCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateProductCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (category is null)
            return Result.Failure(Error.NotFound("ProductCategory.NotFound", "Product Category not found."));

        // Check name uniqueness under same parent if name changed
        if (category.Name != command.Name)
        {
            bool otherExists = await _context.ProductCategories
                .AnyAsync(c => c.Name == command.Name && c.Id != category.Id && c.ParentCategoryId == command.ParentCategoryId, cancellationToken);
            
            if (otherExists)
                return Result.Failure(Error.Conflict("ProductCategory.DuplicateName", $"A category named '{command.Name}' already exists under the same parent category."));
        }

        category.UpdateDetails(command.Name, command.Description, command.DisplayOrder, command.IconUrl, command.Color);

        // Handle Parent Change
        if (category.ParentCategoryId != command.ParentCategoryId)
        {
            ProductCategory? newParent = null;

            if (command.ParentCategoryId.HasValue)
            {
                newParent = await _context.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == command.ParentCategoryId.Value, cancellationToken);

                if (newParent == null)
                    return Result.Failure(Error.NotFound("ProductCategory.ParentNotFound", "New parent category not found."));
            }

            // Load descendants to ensure "ChangeParent" can recurse through them via navigation property fix-up
            // We load them into the context, so EF Core fixes up the 'SubCategories' inverse navigation
            var descendents = await _context.ProductCategories
                .Where(c => c.Path.StartsWith(category.Path) && c.Id != category.Id)
                .ToListAsync(cancellationToken);

            var moveResult = category.ChangeParent(newParent);
            if (moveResult.IsFailure) return moveResult;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
