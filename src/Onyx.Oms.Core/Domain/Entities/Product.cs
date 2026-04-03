using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.Services;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class Product : AuditableEntity<Guid>, IMustHaveTenant
{
    private Product(): base(Guid.NewGuid()) { }

    internal Product(
        Guid tenantId,
        string name,
        string baseSku,
        string? description,
        Guid categoryId,
        Money baseCost,
        Money basePrice,
        Weight? baseWeight,
        bool hasVariants) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        Name = name;
        BaseSku = baseSku;
        Description = description;
        CategoryId = categoryId;
        BasePrice = basePrice;
        BaseCost = baseCost;
        BaseWeight = baseWeight;
        HasVariants = hasVariants;
        IsActive = true;
    }

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string BaseSku { get; private set; } = string.Empty; // Can be auto-generated or set by user
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }

    // Financials & Measurements
    public Money BaseCost { get; private set; } = Money.Zero();
    public Money BasePrice { get; private set; } = Money.Zero();
    public Weight? BaseWeight { get; private set; }

    public bool IsActive { get; private set; }

    public bool HasVariants { get; private set; }
    public ProductVariant? DefaultVariant =>
        HasVariants ? null : _variants.FirstOrDefault(v => !v.IsDeleted && !v.Attributes.Any());

    // Specifications - key -> from category specs
    private Dictionary<string, string> _specifications = new();
    public IReadOnlyDictionary<string, string> Specifications => _specifications.AsReadOnly();

    // Dynamic options
    private readonly List<ProductOption> _options = new();
    public IReadOnlyCollection<ProductOption> Options => _options.AsReadOnly();

    // Navigation
    public virtual ProductCategory Category { get; private set; } = null!;

    private readonly List<ProductVariant> _variants = new();
    public virtual IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private readonly List<ProductImage> _images = new();
    public virtual IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<string> _tags = new();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public static Result<Product> Create(
        Guid tenantId,
        string name,
        string baseSku,
        string? description,
        Guid categoryId,
        List<SpecDefinition> specDefinitions,
        Dictionary<string, string> specifications,
        Money baseCost,
        Money basePrice,
        Weight? baseWeight,
        bool hasVariants,
        List<ProductOption>? options = null,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(Error.Validation("Product.NameRequired", "Product name is required."));

        if (categoryId == Guid.Empty)
            return Result.Failure<Product>(Error.Validation("Product.CategoryRequired", "Category is required."));

        if (hasVariants && (options == null || options.Count == 0))
            return Result.Failure<Product>(Error.Validation("Product.OptionsRequired", "Options are required when there are variants."));

        if (!hasVariants && options != null && options.Count > 0)
            return Result.Failure<Product>(Error.Validation("Product.OptionsNotAllowed", "Options are not allowed when there is no variants."));

        if (hasVariants && options!.Count > 3)
            return Result.Failure<Product>(Error.Validation("Product.TooManyOptions", "A product can have a maximum of 3 options."));

        var product = new Product(
            tenantId,
            name,
            baseSku,
            description,
            categoryId,
            baseCost,
            basePrice,
            baseWeight,
            hasVariants);

        var specResult = product.UpdateSpecifications(specifications, specDefinitions);
        if (specResult.IsFailure)
            return Result.Failure<Product>(specResult.Error);

        if (hasVariants)
        {
            product._options.AddRange(options!);
        }
        else
        {
            // Variant-less mode: create a default variant to hold logistics.
            // This is purely internal — the UI works with simple fields, we store them here.
            var defaultVariantResult = ProductVariant.CreateDefault(
                product,
                baseSku,
                baseCost,
                basePrice,
                baseWeight);

            if (defaultVariantResult.IsFailure)
                return Result.Failure<Product>(defaultVariantResult.Error);

            product._variants.Add(defaultVariantResult.Value);
        }

        if (tags != null && tags.Any())
        {
            product._tags.AddRange(tags);
        }

        return Result.Success(product);
    }

    public Result UpdateBasicInfo(
        string name,
        string? description,
        string? baseSku,
        Guid categoryId,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Product.NameRequired", "Product name is required."));

        if (categoryId == Guid.Empty)
            return Result.Failure(Error.Validation("Product.CategoryRequired", "Category is required."));

        Name = name;
        Description = description;
        if (!string.IsNullOrWhiteSpace(baseSku))
            BaseSku = baseSku.ToUpperInvariant();
            
        CategoryId = categoryId;

        _tags.Clear();
        if (tags != null && tags.Any())
        {
            _tags.AddRange(tags);
        }

        return Result.Success();
    }

    public Result UpdateBaseLogistics(
        Money baseCost,
        Money basePrice,
        Weight? baseWeight)
    {
        BaseCost = baseCost;
        BasePrice = basePrice;
        BaseWeight = baseWeight;

        return Result.Success();
    }

    public Result UpdateSpecifications(Dictionary<string, string> newSpecs, List<SpecDefinition> specDefinitions)
    {
        // Check for required fields
        foreach(var specDef in specDefinitions.Where(sp => sp.IsRequired))
        {
            if(!newSpecs.ContainsKey(specDef.Key) || string.IsNullOrWhiteSpace(newSpecs[specDef.Key]))
                return Result.Failure(Error.Validation("Product.MissingSpec", $"Specification '{specDef.Label}' is required."));
        }

        // Remove junk data
        // Only keep keys that actually exist in the category definition
        var validKeys = specDefinitions.Select(sp => sp.Key).ToHashSet();
        var cleanSpecs = newSpecs
            .Where(kvp => validKeys.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        _specifications = cleanSpecs;
        return Result.Success();
    }

    public Result<List<ProductVariant>> UpdateOptionValues(List<ProductOption> newOptions, List<List<VariantAttribute>> validVariantMatrix, string userId = "System - Option value removed")
    {
        // Update the JSON Document for Options
        _options.Clear();

        // Ensure display order is set correctly
        for (int i = 0; i < newOptions.Count; i++)
        {
            newOptions[i].DisplayOrder = i;
            _options.Add(newOptions[i]);
        }

        // Determine if fundamental axes changed(e.g., "Size, Color"-> "Material")
        bool axesMatch = _options.Count == newOptions.Count &&
            _options.All(o => newOptions.Any(n => n.Name.Equals(o.Name, StringComparison.OrdinalIgnoreCase)));

        // Helper function to create a unique, sort-independent string for a combination of attributes
        // Example output: "Color:Red|Size:XL"
        string GetComboKey(IEnumerable<VariantAttribute> attr) => 
            string.Join("|", attr.OrderBy(a => a.Name).Select(a => $"{a.Name}:{a.Value}"));

        var validMatrixKeys = validVariantMatrix.Select(GetComboKey).ToHashSet();
        var activeVariants = _variants.Where(v => !v.IsDeleted).ToList();

        // HANDLE DELETIONS
        if (!axesMatch)
        {
            // AXES CHANGED: The structure is totally different. Soft-delete ALL active variants.
            foreach (var variant in activeVariants)
            {
                variant.Delete(userId);
            }
        }
        else
        {
            // ONLY VALUES CHANGED: Keep valid variants, soft-delete orphaned ones (e.g., removed "Red")
            foreach (var variant in activeVariants)
            {
                var variantKey = GetComboKey(variant.Attributes);

                if (!validMatrixKeys.Contains(variantKey))
                {
                    variant.Delete(userId); // This combination is no longer valid
                }
            }
        }

        // HANDLE CREATIONS (Generate missing variants)
        // Get the keys of variants that survived the deletion phase
        var survivingVariantKeys = _variants
            .Where(v => !v.IsDeleted)
            .Select(v => GetComboKey(v.Attributes))
            .ToHashSet();

        var newVariants = new List<ProductVariant>();
        foreach (var combo in validVariantMatrix)
        {
            var comboKey = GetComboKey(combo);
            
            // If an active variant doesn't already exist for this combination, create it!
            if (!survivingVariantKeys.Contains(comboKey))
            {
                var suffix = string.Join("-", combo.Select(a => SkuGenerator.GetOptionValueCode(a.Value)));
                string newSku = $"{BaseSku}-{suffix}";

                var cost = new Money(BaseCost.Amount, BaseCost.Currency);
                var price = new Money(BasePrice.Amount, BasePrice.Currency);
                var weight = BaseWeight != null ? new Weight(BaseWeight.Value, BaseWeight.Unit) : null;

                var varinatResult = ProductVariant.Create(
                    this,
                    newSku,
                    combo,
                    cost,
                    price,
                    weight,
                    0
                );
                if (varinatResult.IsFailure)
                    return Result.Failure<List<ProductVariant>>(varinatResult.Error);

                var variant = varinatResult.Value;
                newVariants.Add(variant);
                _variants.Add(variant);
            }
        }

        return newVariants;
    }

    public Result ChangeBaseSku(string newBaseSku)
    {
        if (string.IsNullOrEmpty(newBaseSku))
            return Result.Failure(Error.Validation("Product.SkuRequired", "Product SKU cannot be empty."));

        BaseSku = newBaseSku.ToUpperInvariant();
        return Result.Success();
    }

    public void Activate()
    {
        IsActive = true;
        if (!HasVariants)
        {
            DefaultVariant?.Activate();
        }
    }
    public void Deactivate()
    {
        foreach (var variant in _variants)
            variant.Deactivate();

        IsActive = false;
    }

    // Methods to manage variants/images can be added here or handled via separate aggregates/repos if strict DDD is relaxed for performance.
    public Result ToggleHasVariants(bool hasVariants, string userId)
    {
        if (HasVariants == hasVariants)
            return Result.Success();

        if (hasVariants)
        {
            var defaultVariant = DefaultVariant;
            if (defaultVariant is null)
                defaultVariant = _variants.FirstOrDefault(v => !v.IsDeleted && !v.Attributes.Any());

            if (defaultVariant != null)
                defaultVariant.Delete(userId);

            HasVariants = true;
        }
        else
        {
            foreach (var variant in _variants.Where(v => !v.IsDeleted))
                variant.Delete(userId);

            if(_options.Any())
                _options.Clear();

            HasVariants = false;

            var defaultVariantResult = ProductVariant.CreateDefault(
                this,
                BaseSku,
                BaseCost,
                BasePrice,
                BaseWeight);

            if (defaultVariantResult.IsFailure)
                return defaultVariantResult;

            _variants.Add(defaultVariantResult.Value);
        }

        return Result.Success();
    }

    public Result AddVariant(ProductVariant variant)
    {
        if (!HasVariants)
            return Result.Failure(Error.Validation("Product.NoVariants", "This product does not use variants."));

        var newSignature = string.Join("-", variant.Attributes.OrderBy(a => a.Name).Select(a => a.Value));

        var isDuplicate = _variants
            .Where(v => !v.IsDeleted && v.Id != variant.Id)
            .Any(v => string.Join("-", v.Attributes.OrderBy(a => a.Name).Select(a => a.Value)) == newSignature);

        if(isDuplicate)
            return Result.Failure(Error.Conflict("Product.DuplicateVariant", "A variant with these exact options already exists."));

        _variants.Add(variant);
        return Result.Success();
    }

    /// <summary>
    /// Create the internal default variant for variant-less products.
    /// </summary>
    public Result<ProductVariant> CreateDefaultVariant(int stockOnHand)
    {
        if (HasVariants)
            return Result.Failure<ProductVariant>(Error.Validation("Product.HasVariants", "This product uses variants. Update individual variants directly."));

        var defaultVariant = DefaultVariant;
        if (defaultVariant is not null)
            return Result.Failure<ProductVariant>(Error.NotFound("Product.DefaultVariantExists", "Default variant is already exist."));

        return ProductVariant.CreateDefault(
            this,
            BaseSku, 
            BaseCost, 
            BasePrice, 
            BaseWeight, 
            stockOnHand);
    }

    /// <summary>
    /// Updates the logistics of the internal default variant for variant-less products.
    /// </summary>
    public Result SetDefaultVariantLogistics(
        string sku,
        Money cost,
        Money price,
        Weight? weight,
        int stockOnHand)
    {
        if (HasVariants)
            return Result.Failure(Error.Validation("Product.HasVariants", "This product uses variants. Update individual variants directly."));

        var defaultVariant = DefaultVariant;
        if (defaultVariant is null)
            return Result.Failure(Error.NotFound("Product.DefaultVariantMissing", "Default variant not found."));

        return defaultVariant.UpdateDefaultLogistics(sku, cost, price, weight, stockOnHand);
    }

    public void AddImage(ProductImage image)
    {
        _images.Add(image);
    }

    public Result<List<ProductImage>> UpdateImages(IEnumerable<(Guid Id, string Url, int DisplayOrder, bool IsMain, string? OptionName, string? OptionValue)> updatedImages)
    {
        var incomingIds = updatedImages.Where(i => i.Id != Guid.Empty).Select(i => i.Id).ToHashSet();
        var newImages = new List<ProductImage>();

        _images.RemoveAll(img => !incomingIds.Contains(img.Id));

        foreach (var imgData in updatedImages)
        {
            var existing = _images.FirstOrDefault(i => i.Id == imgData.Id);
            if (existing != null)
            {
                existing.Update(imgData.Url, imgData.DisplayOrder, imgData.IsMain);

                if (!string.IsNullOrEmpty(imgData.OptionName) && !string.IsNullOrEmpty(imgData.OptionValue))
                {
                    var linkResult = existing.LinkToOption(imgData.OptionName, imgData.OptionValue, _options);
                    if (linkResult.IsFailure) return Result.Failure<List<ProductImage>>(linkResult.Error);
                }
                else
                {
                    existing.Unlink();
                }
            }
            else
            {
                var newImage = new ProductImage(Id, imgData.Url, imgData.DisplayOrder, imgData.IsMain);
                if (!string.IsNullOrEmpty(imgData.OptionName) && !string.IsNullOrEmpty(imgData.OptionValue))
                {
                    var linkResult = newImage.LinkToOption(imgData.OptionName, imgData.OptionValue, _options);
                    if (linkResult.IsFailure) return Result.Failure<List<ProductImage>>(linkResult.Error);
                }
                _images.Add(newImage);
                newImages.Add(newImage);
            }
        }

        return newImages;
    }
}
