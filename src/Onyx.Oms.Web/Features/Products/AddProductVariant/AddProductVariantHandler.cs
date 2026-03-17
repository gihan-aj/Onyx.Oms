using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.Services;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.AddProductVariant
{
    public class AddProductVariantHandler : ICommandHandler<AddProductVariantCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public AddProductVariantHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure<Guid>(Error.NotFound("Product.NotFound", "Product not found."));

            var attributes = request.Attributes
                .Select(a => new VariantAttribute
                {
                    Name = a.Name,
                    Value = a.Value,
                }).ToList();

            string? variantSku = request.Sku;
            if (string.IsNullOrWhiteSpace(variantSku))
            {
                var suffix = string.Join("-", attributes.Select(a => SkuGenerator.GetOptionValueCode(a.Value)));
                variantSku = $"{product.BaseSku}-{suffix}";
            }

            var cost = request.Cost != null ? new Money(request.Cost.Amount, request.Cost.Currency) : product.BaseCost;
            var price = request.Price != null ? new Money(request.Price.Amount, request.Price.Currency) : product.BasePrice;
            var weight = request.Weight != null ? new Weight(request.Weight.Value, request.Weight.Unit) : product.BaseWeight;

            // ProductVariant.Create expects a product that has variants
            var variantResult = ProductVariant.Create(
                product,
                variantSku,
                attributes,
                cost,
                price,
                weight,
                request.StockOnHand
            );

            if (variantResult.IsFailure)
                return Result.Failure<Guid>(variantResult.Error);

            var variant = variantResult.Value;

            var addResult = product.AddVariant(variant);
            if (addResult.IsFailure)
                return Result.Failure<Guid>(addResult.Error);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(variant.Id);
        }
    }
}
