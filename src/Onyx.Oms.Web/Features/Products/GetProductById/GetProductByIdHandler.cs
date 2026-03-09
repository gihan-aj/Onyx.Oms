using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GetProductById;

public class GetProductByIdHandler : IQueryHandler<GetProductByIdQuery, ProductDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductDetailsDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(P => P.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDetailsDto>(Error.NotFound("Product.NotFound", "Product not found."));
        }

        int? stockOnHand = product.HasVariants 
            ? product.Variants.Sum(v => v.StockOnHand) 
            : product.DefaultVariant?.StockOnHand;

        int? reservedQuantity = product.HasVariants
            ? product.Variants.Sum(v => v.ReservedQuantity)
            : product.DefaultVariant?.ReservedQuantity;

        var variantsToReturn = product.HasVariants
            ? product.Variants.Select(v => new ProductVariantDto(
                v.Id,
                v.Sku,
                v.Attributes.Select(a => new VariantAttributeDto(a.Name, a.Value)).ToList(),
                v.Cost.Amount,
                v.Cost.Currency,
                v.Price.Amount,
                v.Price.Currency,
                v.Weight?.Value,
                v.Weight?.Unit,
                v.StockOnHand,
                v.ReservedQuantity,
                v.IsActive)).ToList()
            : new List<ProductVariantDto>();

        var allSpecDefs = await BuildAllSpecificationsAsync(product.Category, cancellationToken);

        var specs = allSpecDefs
            .Where(s => product.Specifications.TryGetValue(s.Key, out var value) && !string.IsNullOrWhiteSpace(value))
            .Select(s => new ProductSpecDto(s.Key, s.Label, product.Specifications[s.Key]))
            .ToList();

        var options = product.Options.Select(o => new ProductOptionDto(o.Name, o.DisplayOrder, o.Values)).ToList();

        var dto = new ProductDetailsDto(
            product.Id,
            product.Name,
            product.BaseSku,
            product.Description,
            product.CategoryId,
            product.Category.Name,
            product.Category.NamePath,
            specs,
            product.BaseCost.Amount,
            product.BaseCost.Currency,
            product.BasePrice.Amount,
            product.BasePrice.Currency,
            product.BaseWeight?.Value,
            product.BaseWeight?.Unit,
            product.HasVariants,
            stockOnHand,
            reservedQuantity,
            product.Tags.ToList(),
            options,
            variantsToReturn,
            product.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.DisplayOrder, i.IsMain, i.OptionName, i.OptionValue)).ToList(),
            product.IsActive
        );

        return Result.Success(dto);
    }

    private async Task<List<SpecDefinition>> BuildAllSpecificationsAsync(
        ProductCategory category,
        CancellationToken cancellationToken)
    {
        var ancestorIds = category.Path
            .Split(ProductCategory.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => Guid.TryParse(segment, out var guid) ? guid : (Guid?)null)
            .Where(g => g.HasValue && g.Value != category.Id)
            .Select(g => g!.Value)
            .ToList();

        var merged = new Dictionary<string, SpecDefinition>(StringComparer.OrdinalIgnoreCase);

        if (ancestorIds.Count > 0)
        {
            var ancestors = await _context.ProductCategories
                .AsNoTracking()
                .Where(c => ancestorIds.Contains(c.Id))
                .OrderBy(c => c.Level)
                .ToListAsync(cancellationToken);

            foreach (var ancestor in ancestors)
            {
                foreach (var spec in ancestor.Specifications)
                {
                    merged[spec.Key] = spec;
                }
            }
        }

        foreach (var spec in category.Specifications)
        {
            merged[spec.Key] = spec;
        }

        return [.. merged.Values];
    }
}
