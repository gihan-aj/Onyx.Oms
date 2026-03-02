using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.UpdateProductCategory;

public class UpdateProductCategoryHandler : ICommandHandler<UpdateProductCategoryCommand>
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

        bool nameChanged = category.Name != command.Name;

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

                if(newParent.IsActive == false && category.IsActive)
                    return Result.Failure(Error.Validation("ProductCategory.InvalidParent", "Cannot move an active category under an inactive parent category."));
            }

            // Load descendants to ensure "ChangeParent" can recurse through them via navigation property fix-up
            // We load them into the context, so EF Core fixes up the 'SubCategories' inverse navigation
            var descendents = await _context.ProductCategories
                .Where(c => c.Path.StartsWith(category.Path) && c.Id != category.Id)
                .ToListAsync(cancellationToken);

            var moveResult = category.ChangeParent(newParent);
            if (moveResult.IsFailure) return moveResult;
        }
        else if (nameChanged)
        {
            // Parent didn't change, but Name did. We must update the NamePath of all descendants.
            var descendents = await _context.ProductCategories
                .Where(c => c.Path.StartsWith(category.Path) && c.Id != category.Id)
                .ToListAsync(cancellationToken);

            var updateResult = category.UpdateSubCategoriesPaths();
            if (updateResult.IsFailure) return updateResult;
        }

        if (command.Specifications is not null)
        {
            // TODO: When Product entity is added, check if there are any products under this category.
            // If there are products, return an error and do not allow updating specifications.
            var specUpdateResult = category.UpdateSpecifications(command.Specifications);
            if (specUpdateResult.IsFailure) return specUpdateResult;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
