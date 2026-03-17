using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductSpecifications
{
    public class UpdateProductSpecificationsHandler : ICommandHandler<UpdateProductSpecificationsCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductSpecificationsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateProductSpecificationsCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var specDefinitions = await BuildAllSpecificationsAsync(product.Category, cancellationToken);

            var updateResult = product.UpdateSpecifications(request.Specifications, specDefinitions);

            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        private async Task<List<SpecDefinition>> BuildAllSpecificationsAsync(
            ProductCategory category,
            CancellationToken cancellationToken)
        {
            var ancestorIds = category.Path
                .Split(ProductCategory.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => Guid.TryParse(segment, out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue && g.Value != category.Id)
                .Select(g => g!.Value)
                .ToList();

            var merged = new Dictionary<string, SpecDefinition>(StringComparer.OrdinalIgnoreCase);

            if (ancestorIds.Count > 0)
            {
                var ancestors = await _context.ProductCategories
                    .AsNoTracking()
                    .Where(c => ancestorIds.Contains(c.Id))
                    .OrderBy(c => c.Level)
                    .ToListAsync(cancellationToken);

                foreach (var ancestor in ancestors)
                {
                    foreach (var spec in ancestor.Specifications)
                    {
                        merged[spec.Key] = spec;
                    }
                }
            }

            foreach (var spec in category.Specifications)
            {
                merged[spec.Key] = spec;
            }

            return [.. merged.Values];
        }
    }
}
