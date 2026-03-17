using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateDefaultVariantLogistics
{
    public class UpdateDefaultVariantLogisticsHandler : ICommandHandler<UpdateDefaultVariantLogisticsCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateDefaultVariantLogisticsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateDefaultVariantLogisticsCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var cost = new Money(request.Cost.Amount, request.Cost.Currency);
            var price = new Money(request.Price.Amount, request.Price.Currency);
            var weight = request.Weight != null ? new Weight(request.Weight.Value, request.Weight.Unit) : null;

            var result = product.SetDefaultVariantLogistics(
                request.Sku,
                cost,
                price,
                weight,
                request.StockOnHand);

            if (result.IsFailure)
                return Result.Failure(result.Error);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
