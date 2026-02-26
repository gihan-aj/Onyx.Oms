using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoriesPaged;

public class GetProductCategoriesPagedHandler : IQueryHandler<GetProductCategoriesPagedQuery, PagedResult<ProductCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoriesPagedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ProductCategoryDto>>> Handle(GetProductCategoriesPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ProductCategories
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .AsQueryable();

        // 1. Filtering
        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(c => 
                c.Name.Contains(request.SearchTerm) ||
                (c.Description != null && c.Description.Contains(request.SearchTerm)) ||
                c.NamePath.Contains(request.SearchTerm));
        }

        if (request.IsValidParent.HasValue && request.IsValidParent.Value)
        {
            query = query.Where(c => c.Level < Core.Domain.Entities.ProductCategory.MaxDepth);
        }

        if (request.IsLeafOnly.HasValue && request.IsLeafOnly.Value)
        {
            query = query.Where(c => !c.SubCategories.Any());
        }

        // 2. Sorting
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projections
        var dtoQuery = query.Select(c => new ProductCategoryDto(
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
            c.IsActive,
            c.CreatedOnUtc));

        // 4. Pagination
        var pagedResult = await PagedResult<ProductCategoryDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedResult);
    }

    private static IQueryable<Core.Domain.Entities.ProductCategory> ApplySorting(IQueryable<Core.Domain.Entities.ProductCategory> query, string? sortColumn, string? sortOrder)
    {
        bool isDesc = sortOrder?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            // Default sorting
            return isDesc 
                ? query.OrderByDescending(c => c.Level).ThenByDescending(c => c.DisplayOrder).ThenByDescending(c => c.Name) 
                : query.OrderBy(c => c.Level).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Name);
        }

        return sortColumn.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "level" => isDesc ? query.OrderByDescending(c => c.Level) : query.OrderBy(c => c.Level),
            "displayorder" => isDesc ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder),
            "isactive" => isDesc ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
            "createddate" => isDesc ? query.OrderByDescending(c => c.CreatedOnUtc) : query.OrderBy(c => c.CreatedOnUtc),
            _ => query.OrderBy(c => c.Level).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Name)
        };
    }
}
