using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariantLogistics
{
    public class UpdateProductVariantLogisticsHandler : ICommandHandler<UpdateProductVariantLogisticsCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductVariantLogisticsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateProductVariantLogisticsCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
            if (variant is null)
                return Result.Failure(Error.NotFound("Variant.NotFound", "Variant not found."));

            var variantCost = request.Cost != null ? new Money(request.Cost.Amount, request.Cost.Currency) : null;
            var variantPrice = request.Price != null ? new Money(request.Price.Amount, request.Price.Currency) : null;
            var variantWeight = request.Weight != null ? new Weight(request.Weight.Value, request.Weight.Unit) : null;

            var updateResult = variant.UpdateLogistics(
                product.BaseCost,
                product.BasePrice,
                product.BaseWeight,
                variantCost,
                variantPrice,
                variantWeight);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            if (variant.StockOnHand != request.StockOnHand)
            {
                variant.AdjustStock(request.StockOnHand - variant.StockOnHand);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
