using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.ActivateProduct
{
    public class ActivateProductHandler : ICommandHandler<ActivateProductCommand>
    {
        private readonly IApplicationDbContext _context;

        public ActivateProductHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(ActivateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            product.Activate();

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
