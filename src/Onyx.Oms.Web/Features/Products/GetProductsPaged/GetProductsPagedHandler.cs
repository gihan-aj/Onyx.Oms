using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Web.Features.Products.GetProductsPaged;

public class GetProductsPagedHandler : IRequestHandler<GetProductsPagedQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsPagedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .AsQueryable();

        // 1. Filtering
        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            string term = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(term) || 
                (p.Brand != null && p.Brand.ToLower().Contains(term)) ||
                p.Tags.Any(t => t.ToLower().Contains(term)) ||
                p.Variants.Any(v => v.Sku.ToLower().Contains(term) || v.Name.ToLower().Contains(term))
            );
        }

        // 2. Sorting
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projections
        var dtoQuery = query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.CategoryId,
            p.Category.Name,
            p.Brand,
            p.Material,
            p.Gender,
            p.BasePrice,
            p.IsActive,
            p.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault() ?? p.Images.Select(i => i.Url).FirstOrDefault(),
            p.Tags.ToList(),
            p.Variants.Count,
            p.Variants.Sum(v => v.StockOnHand)
        ));

        // 4. Pagination
        var pagedResult = await PagedResult<ProductDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedResult);
    }

    private static IQueryable<Core.Domain.Entities.Product> ApplySorting(IQueryable<Core.Domain.Entities.Product> query, string? sortColumn, string? sortOrder)
    {
        bool isDesc = sortOrder?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return isDesc ? query.OrderByDescending(p => p.CreatedOnUtc) : query.OrderByDescending(p => p.CreatedOnUtc);
        }

        return sortColumn.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "brand" => isDesc ? query.OrderByDescending(p => p.Brand) : query.OrderBy(p => p.Brand),
            "category" => isDesc ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
            "baseprice" => isDesc ? query.OrderByDescending(p => p.BasePrice) : query.OrderBy(p => p.BasePrice),
            "isactive" => isDesc ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
            "createddate" => isDesc ? query.OrderByDescending(p => p.CreatedOnUtc) : query.OrderBy(p => p.CreatedOnUtc),
            _ => query.OrderByDescending(p => p.CreatedOnUtc)
        };
    }
}
