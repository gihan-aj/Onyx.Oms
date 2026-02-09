using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateProductHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Check Category Exists
        // We could assume it exists, but foreign key constraint would fail.
        // Explicit check is friendlier.
        var categoryExists = await _context.ProductCategories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        
        if (!categoryExists)
        {
            return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "The specified category does not exist."));
        }

        // 2. Check for Duplicate SKUs (in request and in DB)
        var skus = request.Variants.Select(v => v.Sku).Distinct().ToList();
        if (skus.Count != request.Variants.Count)
        {
            return Result.Failure<Guid>(Error.Validation("Product.DuplicateSkusInRequest", "Duplicate SKUs provided in the request."));
        }

        var existingSkus = await _context.ProductVariants
            .Where(v => skus.Contains(v.Sku))
            .Select(v => v.Sku)
            .ToListAsync(cancellationToken);

        if (existingSkus.Any())
        {
            return Result.Failure<Guid>(Error.Conflict("Product.DuplicateSku", $"The following SKUs already exist: {string.Join(", ", existingSkus)}"));
        }

        // 3. Create Product
        var productResult = Product.Create(
            request.Name,
            request.CategoryId,
            request.BasePrice,
            request.BaseCost,
            request.Description,
            request.Brand,
            request.Material,
            request.Gender,
            request.BaseWeight,
            request.Tags);

        if (productResult.IsFailure)
        {
            return Result.Failure<Guid>(productResult.Error);
        }

        var product = productResult.Value;

        // 4. Create Variants
        foreach (var vDto in request.Variants)
        {
            var variantResult = ProductVariant.Create(
                product.Id,
                vDto.Sku,
                vDto.Name,
                vDto.Size,
                vDto.Color,
                vDto.Price,
                vDto.Cost,
                vDto.Weight,
                vDto.StockOnHand);

            if (variantResult.IsFailure)
            {
                return Result.Failure<Guid>(variantResult.Error);
            }

            product.AddVariant(variantResult.Value);
        }

        // 5. Add Images
        foreach (var iDto in request.Images)
        {
            var image = new ProductImage(Guid.NewGuid(), product.Id, iDto.Url, iDto.DisplayOrder, iDto.IsMain);
            product.AddImage(image);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
