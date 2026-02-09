using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.ProductCategories.ActivateProductCategory;

public class ActivateProductCategoryHandler : IRequestHandler<ActivateProductCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateProductCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("ProductCategory.NotFound", "Category not found."));
        }

        if (category.IsActive)
            return Result.Success();

        category.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
