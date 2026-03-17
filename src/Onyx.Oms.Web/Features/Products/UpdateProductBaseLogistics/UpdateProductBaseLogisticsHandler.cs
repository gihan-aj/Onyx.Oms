using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductBaseLogistics
{
    public class UpdateProductBaseLogisticsHandler : ICommandHandler<UpdateProductBaseLogisticsCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductBaseLogisticsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateProductBaseLogisticsCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var baseCost = new Money(request.BaseCost.Amount, request.BaseCost.Currency);
            var basePrice = new Money(request.BasePrice.Amount, request.BasePrice.Currency);
            Weight? baseWeight = request.BaseWeight != null ? new Weight(request.BaseWeight.Value, request.BaseWeight.Unit) : null;

            var updateResult = product.UpdateBaseLogistics(baseCost, basePrice, baseWeight);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
