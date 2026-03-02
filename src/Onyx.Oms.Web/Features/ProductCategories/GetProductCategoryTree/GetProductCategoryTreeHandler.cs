using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryTree;

public class GetProductCategoryTreeHandler : IQueryHandler<GetProductCategoryTreeQuery, List<ProductCategoryTreeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoryTreeHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductCategoryTreeDto>>> Handle(GetProductCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        // MaxDepth is 3 (Root -> L1 -> L2 -> L3), so we eager load all three levels.
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
                .ThenInclude(sc => sc.SubCategories.Where(ssc => ssc.IsActive == isActive).OrderBy(ssc => ssc.DisplayOrder).ThenBy(ssc => ssc.Name))
                .ThenInclude(ssc => ssc.SubCategories.Where(sssc => sssc.IsActive == isActive).OrderBy(sssc => sssc.DisplayOrder).ThenBy(sssc => sssc.Name));
        }
        else
        {
            query = query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Include(c => c.SubCategories.OrderBy(sc => sc.DisplayOrder).ThenBy(sc => sc.Name))
                .ThenInclude(sc => sc.SubCategories.OrderBy(ssc => ssc.DisplayOrder).ThenBy(ssc => ssc.Name))
                .ThenInclude(ssc => ssc.SubCategories.OrderBy(sssc => sssc.DisplayOrder).ThenBy(sssc => sssc.Name));
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
            category.NamePath,
            category.Level,
            category.IconUrl,
            category.Color,
            category.DisplayOrder,
            category.IsActive,
            category.SubCategories.Select(MapToDto).ToList()
        );
    }
}
