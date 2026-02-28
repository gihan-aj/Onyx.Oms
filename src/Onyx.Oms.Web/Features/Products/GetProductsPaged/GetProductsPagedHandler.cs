using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GetProductsPaged;

public class GetProductsPagedHandler : IQueryHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsPagedHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .AsNoTracking();

        // 1. Filtering
        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.HasColor.HasValue)
        {
            query = query.Where(p => p.HasColor == request.HasColor.Value);
        }

        if (request.HasSize.HasValue)
        {
            query = query.Where(p => p.HasSize == request.HasSize.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm;
            // Case-insensitive filtering using EF.Functions.Like for more flexibility and robust translation
            query = query.Where(p => 
                EF.Functions.Like(p.Name, $"%{searchTerm}%") || 
                EF.Functions.Like(p.BaseSku, $"%{searchTerm}%") ||
                (p.Brand != null && EF.Functions.Like(p.Brand, $"%{searchTerm}%")));
        }

        // 2. Sorting
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projections
        var dtoQuery = query.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.BaseSku,
            p.Category.Name,
            p.Brand,
            p.IsActive,
            p.HasColor,
            p.HasSize,
            p.BasePrice.Amount,
            p.BasePrice.Currency,
            p.BaseCost.Amount,
            p.BaseCost.Currency,
            p.Variants.Sum(v => (int?)v.StockOnHand) ?? 0, // Sum total stock
            p.CreatedOnUtc
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
            "basesku" => isDesc ? query.OrderByDescending(p => p.BaseSku) : query.OrderBy(p => p.BaseSku),
            "category" => isDesc ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
            "brand" => isDesc ? query.OrderByDescending(p => p.Brand) : query.OrderBy(p => p.Brand),
            "isactive" => isDesc ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
            "createddate" => isDesc ? query.OrderByDescending(p => p.CreatedOnUtc) : query.OrderBy(p => p.CreatedOnUtc),
            _ => query.OrderByDescending(p => p.CreatedOnUtc)
        };
    }
}
