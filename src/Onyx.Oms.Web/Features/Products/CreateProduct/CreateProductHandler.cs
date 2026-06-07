using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.Services;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.CreateProduct
{
    public class CreateProductHandler : ICommandHandler<CreateProductCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAppSequenceService _appSequenceService;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductHandler(IApplicationDbContext context, IAppSequenceService appSequenceService, ICurrentUserService currentUserService)
        {
            _context = context;
            _appSequenceService = appSequenceService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
            Guid? tenantId = _currentUserService.ActiveTenantId;
            if (tenantId == null)
                return Result.Failure<Guid>(Error.Unauthorized("Product.TenantIdMissing", "Tenant Id not found."));

            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken);
            if(category is null)
                return Result.Failure<Guid>(Error.NotFound("ProductCategory.NotFound","Product category not found."));

            var specDefinitions = await BuildAllSpecificationsAsync(category, cancellationToken);

            string? baseSku = command.BaseSku;
            if (string.IsNullOrWhiteSpace(baseSku))
            {
                var baseSkuResult = await _appSequenceService.GetNextNumberAsync(Prefixes.Sku, cancellationToken);
                if(baseSkuResult.IsFailure)
                    return Result.Failure<Guid>(baseSkuResult.Error);

                baseSku = baseSkuResult.Value;
            }

            var baseCost = new Money(command.BaseCost.Amount, command.BaseCost.Currency);
            var basePrice = new Money(command.BasePrice.Amount, command.BasePrice.Currency);

            Weight? baseWeight = null;
            if (command.BaseWeight != null)
                baseWeight = new Weight(command.BaseWeight.Value, command.BaseWeight.Unit);

            bool hasVariants = command.HasVariants;

            List<ProductOption>? options = null;
            if(hasVariants && command.Options != null)
                options = command.Options.Select(o => new ProductOption 
                {
                    Name = o.Name,
                    Values = o.Values,
                }).ToList();

            var productResult = Product.Create(
                tenantId.Value,
                command.Name,
                baseSku,
                command.Description,
                command.CategoryId,
                specDefinitions ?? new List<SpecDefinition>(),
                command.Specifications,
                baseCost,
                basePrice,
                baseWeight,
                hasVariants,
                options,
                command.Tags
            );
            if (productResult.IsFailure)
                return Result.Failure<Guid>(productResult.Error);

            var product = productResult.Value;

            if (hasVariants)
            {
                foreach(var variantDto in command.Variants)
                {
                    var attributes = variantDto.Attributes
                        .Select(a => new VariantAttribute
                        {
                            Name = a.Name,
                            Value = a.Value,
                        }).ToList();

                    string? variantSku = variantDto.Sku;
                    if (string.IsNullOrWhiteSpace(variantSku))
                    {
                        // Simple generator: BaseSku + first 3 chars of each attribute value
                        var suffix = string.Join("-", attributes.Select(a => SkuGenerator.GetOptionValueCode(a.Value)));
                        variantSku = $"{baseSku}-{suffix}";
                    }

                    var cost = variantDto.Cost != null ? new Money(variantDto.Cost.Amount, variantDto.Cost.Currency) : baseCost;
                    var price = variantDto.Price != null ? new Money(variantDto.Price.Amount, variantDto.Price.Currency) : basePrice;
                    var weight = variantDto.Weight != null ? new Weight(variantDto.Weight.Value, variantDto.Weight.Unit) : baseWeight;

                    var varinatResult = ProductVariant.Create(
                        tenantId.Value,
                        product,
                        variantSku,
                        attributes,
                        cost,
                        price,
                        weight,
                        variantDto.StockOnHand
                    );
                    if (varinatResult.IsFailure)
                        return Result.Failure<Guid>(varinatResult.Error);

                    var variant = varinatResult.Value;

                    var variantAddResult = product.AddVariant(variant);
                    if (variantAddResult.IsFailure)
                        return Result.Failure<Guid>(variantAddResult.Error);
                }
            }
            else
            {
                var updateResult = product.SetDefaultVariantLogistics(baseSku, baseCost, basePrice, baseWeight, command.BaseStockOnHand ?? 0);
    
                if (updateResult.IsFailure)
                    return Result.Failure<Guid>(updateResult.Error);
            }

            if (command.Images.Any())
            {
                foreach(var imgDto in command.Images)
                {
                    var image = new ProductImage(tenantId.Value, product.Id, imgDto.Url, imgDto.DisplayOrder, imgDto.IsMain);
                    if (!string.IsNullOrWhiteSpace(imgDto.OptionName) && !string.IsNullOrWhiteSpace(imgDto.OptionValue))
                    {
                        var imageResult = image.LinkToOption(imgDto.OptionName, imgDto.OptionValue, product.Options);
                        if (imageResult.IsFailure)
                            return Result.Failure<Guid>(imageResult.Error);
                    }

                    product.AddImage(image);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }

        private async Task<List<SpecDefinition>> BuildAllSpecificationsAsync(
            ProductCategory category,
            CancellationToken cancellationToken)
        {
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
}
