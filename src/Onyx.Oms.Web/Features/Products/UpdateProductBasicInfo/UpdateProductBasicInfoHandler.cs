using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductBasicInfo
{
    public class UpdateProductBasicInfoHandler : ICommandHandler<UpdateProductBasicInfoCommand>
    {
        private readonly IApplicationDbContext _context;
        public UpdateProductBasicInfoHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateProductBasicInfoCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var categoryExists = await _context.ProductCategories
                .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            
            if (!categoryExists)
                return Result.Failure(Error.NotFound("ProductCategory.NotFound", "Product category not found."));

            if (!string.IsNullOrWhiteSpace(request.BaseSku))
            {
                // Check if SKU is unique among other products
                var skuExists = await _context.Products
                    .AnyAsync(p => p.BaseSku == request.BaseSku && p.Id != request.Id, cancellationToken);

                if (skuExists)
                    return Result.Failure(Error.Conflict("Product.SkuConflict", "A product with this Base SKU already exists."));
            }

            var updateResult = product.UpdateBasicInfo(
                request.Name,
                request.Description,
                request.BaseSku,
                request.CategoryId,
                request.Tags);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
