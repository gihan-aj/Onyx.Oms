using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GenerateProductSheet
{
    public class GenerateProductSheetHandler : IQueryHandler<GenerateProductSheetQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IProductSheetGenerator _pdfGenerator;

        public GenerateProductSheetHandler(IProductSheetGenerator pdfGenerator, IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _pdfGenerator = pdfGenerator;
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<byte[]>> Handle(GenerateProductSheetQuery request, CancellationToken cancellationToken)
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == _currentUserService.ActiveTenantId, cancellationToken);

            if (tenant == null)
                return Result.Failure<byte[]>(Error.NotFound("Tenant.NotFound", "Tenant profile not found."));

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                return Result.Failure<byte[]>(Error.NotFound("Product.NotFound", "Product not found."));

            var allSpecDefs = await BuildAllSpecificationsAsync(product.Category, cancellationToken);

            var specs = allSpecDefs
                .Where(s => product.Specifications.TryGetValue(s.Key, out var value) && !string.IsNullOrWhiteSpace(value))
                .ToList();

            string logoStoragePath = Path.GetFullPath(Path.Combine(request.ImageStoragePath, "..", "StoreAssets"));

            try
            {
                var pdfBytes = _pdfGenerator.Generate(product, allSpecDefs, tenant, request.ImageStoragePath, logoStoragePath);
                return Result.Success(pdfBytes);
            }
            catch (Exception ex)
            {
                return Result.Failure<byte[]>(Error.Failure("Pdf.GenerationFailed", $"Failed to generate PDF: {ex.Message}"));
            }
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
