using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoriesList;

public class GetProductCategoriesListHandler : IRequestHandler<GetProductCategoriesListQuery, Result<List<ProductCategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoriesListHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductCategoryDto>>> Handle(GetProductCategoriesListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProductCategories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .AsQueryable();

        if (request.OnlyLeaves)
        {
            query = query.Where(c => !c.SubCategories.Any());
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var categories = await query
            .OrderBy(c => c.Level) // Or sorting by NamePath?
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ProductCategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.ParentCategoryId,
                c.ParentCategory != null ? c.ParentCategory.Name : null,
                c.Level,
                c.Path,
                c.NamePath,
                c.IconUrl,
                c.Color,
                c.DisplayOrder,
                c.IsActive
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(categories);
    }
}
