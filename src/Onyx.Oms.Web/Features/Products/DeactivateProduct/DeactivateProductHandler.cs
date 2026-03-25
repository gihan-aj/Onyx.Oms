using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.DeactivateProduct
{
    public class DeactivateProductHandler : ICommandHandler<DeactivateProductCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeactivateProductHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants) // Include variants because Deactivate() cascades to variants
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            product.Deactivate();

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
