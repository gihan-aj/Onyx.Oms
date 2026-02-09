using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryTree;

public class GetProductCategoryTreeHandler : IRequestHandler<GetProductCategoryTreeQuery, Result<List<ProductCategoryTreeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoryTreeHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductCategoryTreeDto>>> Handle(GetProductCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        // MaxDepth is 2 (Root -> Sub -> SubSub), so we can eager load efficiently.
        // Loading only roots and including their children recursively.
        var query = _context.ProductCategories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == null);

        if (request.IsActive.HasValue)
        {
            bool isActive = request.IsActive.Value;
            query = query.Where(c => c.IsActive == isActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Include(c => c.SubCategories.Where(sc => sc.IsActive == isActive).OrderBy(sc => sc.DisplayOrder).ThenBy(sc => sc.Name))
                .ThenInclude(sc => sc.SubCategories.Where(ssc => ssc.IsActive == isActive).OrderBy(ssc => ssc.DisplayOrder).ThenBy(ssc => ssc.Name));
        }
        else
        {
            query = query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Include(c => c.SubCategories.OrderBy(sc => sc.DisplayOrder).ThenBy(sc => sc.Name))
                .ThenInclude(sc => sc.SubCategories.OrderBy(ssc => ssc.DisplayOrder).ThenBy(ssc => ssc.Name));
        }

        var roots = await query.ToListAsync(cancellationToken);

        var dtos = roots.Select(MapToDto).ToList();

        return Result.Success(dtos);
    }

    private static ProductCategoryTreeDto MapToDto(ProductCategory category)
    {
        return new ProductCategoryTreeDto(
            category.Id,
            category.Name,
            category.Description,
            category.Level,
            category.IconUrl,
            category.Color,
            category.DisplayOrder,
            category.IsActive,
            category.SubCategories.Select(MapToDto).ToList()
        );
    }
}
