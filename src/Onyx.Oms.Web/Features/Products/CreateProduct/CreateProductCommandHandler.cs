using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.Services;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IAppSequenceService _sequenceService;

    public CreateProductCommandHandler(IApplicationDbContext context, IAppSequenceService sequenceService)
    {
        _context = context;
        _sequenceService = sequenceService;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // 1. Tenant Configuration Validation
        var tenant = await _context.TenantProfiles.FirstOrDefaultAsync(cancellationToken);
        if (tenant == null)
            return Result.Failure<Guid>(Error.Failure("Tenant.NotFound", "Tenant configuration not found."));

        if (!string.Equals(request.BaseCostCurrency, tenant.BaseCurrency, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(request.BasePriceCurrency, tenant.BaseCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<Guid>(Error.Validation("Product.InvalidCurrency", $"Currency must be the configured base currency: {tenant.BaseCurrency}"));
        }

        if (!string.Equals(request.BaseWeightUnit, tenant.WeightUnit, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<Guid>(Error.Validation("Product.InvalidWeightUnit", $"Weight unit must be the configured base unit: {tenant.WeightUnit}"));
        }

        // 2. Category Validation
        var categoryExists = await _context.ProductCategories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "The specified category does not exist."));

        // 3. Base SKU Generation & Uniqueness Validation
        string? finalBaseSku = request.BaseSku;
        if (string.IsNullOrWhiteSpace(finalBaseSku))
        {
            // PROD sequence based on docs
            finalBaseSku = await _sequenceService.GetNextNumberAsync("PROD", "PROD", cancellationToken);
        }
        else
        {
            var baseSkuExists = await _context.Products.AnyAsync(p => p.BaseSku == finalBaseSku, cancellationToken);
            if (baseSkuExists)
                return Result.Failure<Guid>(Error.Conflict("Product.DuplicateSku", "A product with this Base SKU already exists."));
        }

        // 4. Create Product Aggregate
        var baseCost = new Money(request.BaseCostAmount, request.BaseCostCurrency);
        var basePrice = new Money(request.BasePriceAmount, request.BasePriceCurrency);
        var baseWeight = new Weight(request.BaseWeightValue, request.BaseWeightUnit);

        var productResult = Product.Create(
            request.Name,
            finalBaseSku,
            request.Description,
            request.CategoryId,
            request.Brand,
            request.Material,
            request.Gender,
            baseCost,
            basePrice,
            baseWeight,
            request.HasColor,
            request.HasSize,
            request.Tags);

        if (productResult.IsFailure)
            return Result.Failure<Guid>(productResult.Error);

        var product = productResult.Value;

        // 5. Variants Validation & Creation
        if (request.Variants != null && request.Variants.Any())
        {
            var skusToProcess = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var variantDto in request.Variants)
            {
                // Structure validations based on Domain Flags
                if (product.HasColor && string.IsNullOrWhiteSpace(variantDto.Color))
                    return Result.Failure<Guid>(Error.Validation("ProductVariant.ColorRequired", "This product requires variants to have a color."));
                
                if (product.HasSize && string.IsNullOrWhiteSpace(variantDto.Size))
                    return Result.Failure<Guid>(Error.Validation("ProductVariant.SizeRequired", "This product requires variants to have a size."));

                // SKU Generation & local uniqueness
                string sku = variantDto.Sku;
                if (string.IsNullOrWhiteSpace(sku))
                {
                    sku = SkuGenerator.GenerateVariantSku(product.BaseSku, variantDto.Color, variantDto.Size);
                }

                if (!skusToProcess.Add(sku))
                {
                     return Result.Failure<Guid>(Error.Conflict("ProductVariant.DuplicateSkuInRequest", $"Duplicate variant SKU in request payload: {sku}"));
                }

                var variantCost = variantDto.CostAmount.HasValue ? new Money(variantDto.CostAmount.Value, tenant.BaseCurrency) : (Money?)null;
                var variantPrice = variantDto.PriceAmount.HasValue ? new Money(variantDto.PriceAmount.Value, tenant.BaseCurrency) : (Money?)null;
                var variantWeight = variantDto.WeightValue.HasValue ? new Weight(variantDto.WeightValue.Value, tenant.WeightUnit) : (Weight?)null;

                var variantResult = ProductVariant.Create(
                    product.Id,
                    sku,
                    variantDto.Color,
                    variantDto.Size,
                    baseCost,
                    basePrice,
                    baseWeight,
                    variantCost,
                    variantPrice,
                    variantWeight,
                    variantDto.StockOnHand);

                if (variantResult.IsFailure)
                    return Result.Failure<Guid>(variantResult.Error);

                product.AddVariant(variantResult.Value);
            }

            // Database Uniqueness Validation for SKUs
            var requestedSkus = skusToProcess.ToList();
            var existingVariants = await _context.ProductVariants
                .Where(v => requestedSkus.Contains(v.Sku))
                .Select(v => v.Sku)
                .ToListAsync(cancellationToken);

            if (existingVariants.Any())
                return Result.Failure<Guid>(Error.Conflict("ProductVariant.DuplicateSku", $"Variants with the following SKUs already exist in the database: {string.Join(", ", existingVariants)}"));
        }
        else
        {
             // If creating structural product, forces at least one variant unless its a draft. 
             // Depending on business rules we might allow empty variants. Doing no block here. 
        }

        // 6. Image Validations & Creation
        if (request.Images != null && request.Images.Any())
        {
            // Collect proposed variant colors
            var validColors = request.Variants?.Where(v => !string.IsNullOrEmpty(v.Color)).Select(v => v.Color!).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();

            foreach (var imageDto in request.Images)
            {
                var image = new ProductImage(
                    Guid.NewGuid(),
                    product.Id,
                    imageDto.Url,
                    imageDto.DisplayOrder,
                    imageDto.IsMain
                );

                if (!string.IsNullOrWhiteSpace(imageDto.Color))
                {
                    if (!product.HasColor)
                        return Result.Failure<Guid>(Error.Validation("ProductImage.InvalidColorTag", "Cannot tag image with color because the product does not have colors enabled."));
                    
                    if (!validColors.Contains(imageDto.Color, StringComparer.OrdinalIgnoreCase))
                        return Result.Failure<Guid>(Error.Validation("ProductImage.ColorMismatch", $"The image color tag '{imageDto.Color}' does not match any variants in this request."));

                    var colorResult = image.TagWithColor(imageDto.Color);
                    if (colorResult.IsFailure) return Result.Failure<Guid>(colorResult.Error);
                }

                product.AddImage(image);
            }
        }

        // 7. Persistence
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
