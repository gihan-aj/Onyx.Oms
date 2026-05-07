using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;

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
            .AsSplitQuery();

        // Stock filtering
        if(request.StockFilterStatus != StockFilterStatus.All)
        {
            query = request.StockFilterStatus switch
            {
                StockFilterStatus.InStock => query.Where(p => p.Variants.Any(v => v.IsActive && (v.StockOnHand - v.ReservedQuantity) > 0)),
                StockFilterStatus.LowStock => query.Where(p => p.Variants.Any(v => v.IsActive && (v.StockOnHand - v.ReservedQuantity) > 0 && (v.StockOnHand - v.ReservedQuantity) <= 10)),
                StockFilterStatus.OutOfStock => query.Where(p => p.Variants.Any(v => v.IsActive && (v.StockOnHand - v.ReservedQuantity) <= 0)),
                _ => query
            };
        }

        // Filtering
        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (request.CategoryId.HasValue)
        {
            //query = query.Where(p => p.CategoryId == request.CategoryId.Value);
            query = query.Where(p => p.Category.Path.Contains(request.CategoryId.Value.ToString()));
        }

        if (request.HasVariants.HasValue)
        {
            query = query.Where(p => p.HasVariants == request.HasVariants.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            // Search in Name, BaseSku, Description, or Tags
            query = query.Where(p => 
                p.Name.Contains(request.SearchTerm) ||
                p.BaseSku.Contains(request.SearchTerm) ||
                (p.Description != null && p.Description.Contains(request.SearchTerm)) ||
                p.Tags.Any(t => t.Contains(request.SearchTerm)));
        }

        // 2. Sorting
        query = ApplySorting(query, request.SortColumn, request.SortOrder);

        // 3. Projections
        IQueryable<ProductDto> dtoQuery;
        if(request.IncludeVariantsAndImages.HasValue && request.IncludeVariantsAndImages.Value)
        {
            dtoQuery = query.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.BaseSku,
                p.CategoryId,
                p.Category.Name,
                p.Category.NamePath,
                p.BasePrice.Amount,
                p.BasePrice.Currency,
                p.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault(),
                p.HasVariants,
                p.Options.Select(o => new ProductOptionDto(o.Name, o.DisplayOrder, o.Values)).ToList(),
                p.Variants.Where(v => v.IsActive)
                    .Select(v => new ProductVariantDto(
                        v.Id,
                        v.Sku,
                        v.Attributes.Select(a => new VariantAttributeDto(a.Name, a.Value)).ToList(),
                        v.Cost.Amount,
                        v.Cost.Currency,
                        v.Price.Amount,
                        v.Price.Currency,
                        v.Weight != null ? v.Weight.Value : null,
                        v.Weight != null ? v.Weight.Unit : null,
                        v.StockOnHand,
                        v.ReservedQuantity,
                        v.IncomingStock,
                        v.IsActive))
                    .ToList(),
                p.Images
                    .Select(i => new ProductImageDto(
                        i.Id,
                        i.Url,
                        i.DisplayOrder,
                        i.IsMain,
                        i.OptionName,
                        i.OptionValue))
                    .ToList(),
                p.Variants.Where(v => v.IsActive).Sum(v => v.StockOnHand),
                p.Variants.Where(v => v.IsActive).Sum(v => v.StockOnHand - v.ReservedQuantity),
                p.IsActive,
                p.CreatedOnUtc,
                p.LastModifiedOnUtc));
        }
        else
        {
            dtoQuery = query.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.BaseSku,
                p.CategoryId,
                p.Category.Name,
                p.Category.NamePath,
                p.BasePrice.Amount,
                p.BasePrice.Currency,
                p.Images.Where(i => i.IsMain).Select(i => i.Url).FirstOrDefault(),
                p.HasVariants,
                p.Options.Select(o => new ProductOptionDto(o.Name, o.DisplayOrder, o.Values)).ToList(),
                null,
                null,
                p.Variants.Where(v => v.IsActive).Sum(v => v.StockOnHand),
                p.Variants.Where(v => v.IsActive).Sum(v => v.StockOnHand - v.ReservedQuantity),
                p.IsActive,
                p.CreatedOnUtc,
                p.LastModifiedOnUtc));
        } 

        // 4. Pagination
        var pagedResult = await PagedResult<ProductDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

        return Result.Success(pagedResult);
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortColumn, string? sortOrder)
    {
        bool isDesc = sortOrder?.ToLower() == "desc";

        if (string.IsNullOrWhiteSpace(sortColumn))
        {
            return isDesc ? query.OrderByDescending(p => p.CreatedOnUtc) : query.OrderBy(p => p.CreatedOnUtc);
        }

        return sortColumn.ToLower() switch
        {
            "name" => isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "basesku" => isDesc ? query.OrderByDescending(p => p.BaseSku) : query.OrderBy(p => p.BaseSku),
            "categoryname" => isDesc ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
            "baseprice" => isDesc ? query.OrderByDescending(p => p.BasePrice.Amount) : query.OrderBy(p => p.BasePrice.Amount),
            "isactive" => isDesc ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
            "createddate" => isDesc ? query.OrderByDescending(p => p.CreatedOnUtc) : query.OrderBy(p => p.CreatedOnUtc),
            _ => query.OrderByDescending(p => p.CreatedOnUtc)
        };
    }
}
