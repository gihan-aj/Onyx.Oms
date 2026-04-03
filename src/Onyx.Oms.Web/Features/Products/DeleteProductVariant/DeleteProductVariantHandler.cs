using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.DeleteProductVariant
{
    public class DeleteProductVariantHandler : ICommandHandler<DeleteProductVariantCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProductVariantHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
            if (variant is null)
                return Result.Failure(Error.NotFound("Variant.NotFound", "Variant not found."));

            var userId = _currentUserService.UserId;
            
            variant.Delete(userId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
