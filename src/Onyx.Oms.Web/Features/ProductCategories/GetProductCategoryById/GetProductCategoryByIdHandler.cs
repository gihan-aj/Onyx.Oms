using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryById;

public class GetProductCategoryByIdHandler : IQueryHandler<GetProductCategoryByIdQuery, ProductCategoryResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoryByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductCategoryResponse>> Handle(GetProductCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure<ProductCategoryResponse>(Error.NotFound("ProductCategory.NotFound", "Product Category not found."));
        }

        var allSpecifications = await BuildAllSpecificationsAsync(category, request.IncludeParentSpecs, cancellationToken);

        var response = new ProductCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            category.Level,
            category.Path,
            category.NamePath,
            category.IsActive,
            category.DisplayOrder,
            category.IconUrl,
            category.Color,
            category.Specifications.ToList(),
            allSpecifications
        );

        return Result.Success(response);
    }

    /// <summary>
    /// When <paramref name="includeParentSpecs"/> is true, parses ancestor IDs from the
    /// materialized Path, fetches all ancestors in a single query, then merges their specs
    /// root-first. Child-level specs override parent specs for the same Key.
    /// Returns an empty list when the flag is false.
    /// </summary>
    private async Task<List<SpecDefinition>> BuildAllSpecificationsAsync(
        ProductCategory category,
        bool includeParentSpecs,
        CancellationToken cancellationToken)
    {
        if (!includeParentSpecs)
            return [];

        // The materialized path looks like: /rootId/childId/leafId/
        // Split and parse all non-empty segments as ancestor GUIDs (excluding the category itself).
        var ancestorIds = category.Path
            .Split(ProductCategory.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => Guid.TryParse(segment, out var guid) ? guid : (Guid?)null)
            .Where(g => g.HasValue && g.Value != category.Id)
            .Select(g => g!.Value)
            .ToList();

        // Merged dictionary: key = SpecDefinition.Key, ordered root-first so child overrides parent.
        var merged = new Dictionary<string, SpecDefinition>(StringComparer.OrdinalIgnoreCase);

        if (ancestorIds.Count > 0)
        {
            // Single round-trip to fetch all ancestor categories.
            var ancestors = await _context.ProductCategories
                .AsNoTracking()
                .Where(c => ancestorIds.Contains(c.Id))
                .OrderBy(c => c.Level)   // root (Level 0) first
                .ToListAsync(cancellationToken);

            foreach (var ancestor in ancestors)
            {
                foreach (var spec in ancestor.Specifications)
                {
                    merged[spec.Key] = spec;  // child levels will overwrite parent values
                }
            }
        }

        // Finally overlay the requested category's own specs (highest priority).
        foreach (var spec in category.Specifications)
        {
            merged[spec.Key] = spec;
        }

        return [.. merged.Values];
    }
}
