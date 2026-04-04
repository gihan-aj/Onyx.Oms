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
            Guid? userId = _currentUserService.UserId;
            if (userId == null)
                return Result.Failure<Guid>(Error.Unauthorized("ProductVariant.TenantIdMissing", "User Id not found."));

            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
            if (variant is null)
                return Result.Failure(Error.NotFound("Variant.NotFound", "Variant not found."));
            
            variant.Delete(userId.Value);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
