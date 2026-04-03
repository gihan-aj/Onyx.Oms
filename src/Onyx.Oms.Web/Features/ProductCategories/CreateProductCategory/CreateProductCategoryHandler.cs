using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.CreateProductCategory;

public class CreateProductCategoryHandler : ICommandHandler<CreateProductCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCategoryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name under the same parent
        bool nameExists = await _context.ProductCategories
            .AnyAsync(c => c.Name == request.Name && c.ParentCategoryId == request.ParentCategoryId, cancellationToken);
        
        if (nameExists)
        {
            return Result.Failure<Guid>(Error.Conflict("ProductCategory.DuplicateName", $"A category named '{request.Name}' already exists under the same parent category."));
        }

        ProductCategory? parentCategory = null;
        if (request.ParentCategoryId.HasValue)
        {
            parentCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == request.ParentCategoryId.Value, cancellationToken);

            if (parentCategory is null)
            {
                return Result.Failure<Guid>(Error.NotFound("ProductCategory.ParentNotFound", "Parent category not found."));
            }
        }

        var result = ProductCategory.Create(
            _currentUserService.ActiveTenantId,
            request.Name,
            request.Description,
            parentCategory,
            request.DisplayOrder,
            request.IconUrl,
            request.Color,
            request.Specifications
        );

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        _context.ProductCategories.Add(result.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
