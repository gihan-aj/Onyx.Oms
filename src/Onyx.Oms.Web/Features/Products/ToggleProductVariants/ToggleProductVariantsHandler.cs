using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.ToggleProductVariants
{
    public class ToggleProductVariantsHandler : ICommandHandler<ToggleProductVariantsCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ToggleProductVariantsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(ToggleProductVariantsCommand request, CancellationToken cancellationToken)
        {
            Guid? userId = _currentUserService.UserId;
            if (userId == null)
                return Result.Failure<Guid>(Error.Unauthorized("ProductVariant.TenantIdMissing", "User Id not found."));

            var product = await _context.Products
                .Include(p => p.Variants) // Needed to map and soft delete existing variants
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var toggleResult = product.ToggleHasVariants(request.HasVariants, userId.Value);

            if (toggleResult.IsFailure)
                return Result.Failure(toggleResult.Error);

            if(!product.HasVariants && product.DefaultVariant != null)
            {
                _context.ProductVariants.Add(product.DefaultVariant);
            }
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
