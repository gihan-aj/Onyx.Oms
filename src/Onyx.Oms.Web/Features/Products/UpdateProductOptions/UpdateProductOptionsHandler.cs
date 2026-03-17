using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductOptions
{
    public class UpdateProductOptionsHandler : ICommandHandler<UpdateProductOptionsCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductOptionsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateProductOptionsCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants) // Required for UpdateOptionValues to validate the deletion of existing variants
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var options = request.Options.Select(o => new ProductOption
            {
                Name = o.Name,
                Values = o.Values,
            }).ToList();

            var userId = _currentUserService.UserId ?? "System - Option value removed";

            var updateResult = product.UpdateOptionValues(options, userId);
            
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
