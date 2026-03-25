using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.Services;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;
using System.Xml.Linq;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariants
{
    public class UpdateProductVariantsHandler : ICommandHandler<UpdateProductVariantsCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductVariantsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateProductVariantsCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            foreach(var updatedVariantDetails in request.Variants)
            {
                var originalVariant = product.Variants.FirstOrDefault(v => v.Id == updatedVariantDetails.Id);
                if (originalVariant is null)
                    return Result.Failure(Error.NotFound("Variant.NotFound", "Variant not found."));

                if(updatedVariantDetails.Sku != originalVariant.Sku)
                {
                    string? newVariantSku = updatedVariantDetails.Sku;
                    if (string.IsNullOrWhiteSpace(newVariantSku))
                    {
                        var suffix = string.Join("-", originalVariant.Attributes.Select(a => SkuGenerator.GetOptionValueCode(a.Value)));
                        newVariantSku = $"{product.BaseSku}-{suffix}";
                    }
                    bool skuExists = await _context.ProductVariants.AnyAsync(v => v.Sku == newVariantSku && v.Id != originalVariant.Id, cancellationToken);
                    if(skuExists)
                        return Result.Failure(Error.Conflict("ProductVariant.SkuConflict", $"A product variant with this SKU: {newVariantSku}, already exists."));

                    var changeSkuResult = originalVariant.ChangeSku(newVariantSku);
                    if (changeSkuResult.IsFailure)
                        return changeSkuResult;
                }

                var variantCost = updatedVariantDetails.Cost != null ? new Money(updatedVariantDetails.Cost.Amount, updatedVariantDetails.Cost.Currency) : null;
                var variantPrice = updatedVariantDetails.Price != null ? new Money(updatedVariantDetails.Price.Amount, updatedVariantDetails.Price.Currency) : null;
                var variantWeight = updatedVariantDetails.Weight != null ? new Weight(updatedVariantDetails.Weight.Value, updatedVariantDetails.Weight.Unit) : null;

                var updateResult = originalVariant.UpdateLogistics(
                    product.BaseCost,
                    product.BasePrice,
                    product.BaseWeight,
                    variantCost,
                    variantPrice,
                    variantWeight);

                if (updateResult.IsFailure)
                    return Result.Failure(updateResult.Error);

                if (updatedVariantDetails.StockOnHand != originalVariant.StockOnHand)
                {
                    originalVariant.AdjustStock(updatedVariantDetails.StockOnHand - originalVariant.StockOnHand);
                }

                if(updatedVariantDetails.IsActive != originalVariant.IsActive)
                {
                    if(updatedVariantDetails.IsActive)
                        originalVariant.Activate();

                    else
                        originalVariant.Deactivate();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
