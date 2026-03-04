using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class Product : AuditableEntity<Guid>
{
    private Product() { }

    internal Product(
        Guid id,
        string name,
        string baseSku,
        string? description,
        Guid categoryId,
        Money baseCost,
        Money basePrice,
        Weight? baseWeight,
        bool hasVariants) : base(id)
    {
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

    public string Name { get; private set; } = string.Empty;
    public string BaseSku { get; private set; } = string.Empty; // Can be auto-generated or set by user
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }

    // Financials & Measurements
    public Money BaseCost { get; private set; } = Money.Zero();
    public Money BasePrice { get; private set; } = Money.Zero();
    public Weight? BaseWeight { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>
    /// When false, this product has no selectable options. Logistics (SKU, price, cost, weight, stock)
    /// are stored in a single internal default variant with empty attributes.
    /// The UI should hide the variant matrix and expose simple logistics fields instead.
    /// </summary>
    public bool HasVariants { get; private set; }

    /// <summary>
    /// Returns the default (no-attribute) variant for variant-less products. Null when HasVariants is true.
    /// </summary>
    public ProductVariant? DefaultVariant =>
        HasVariants ? null : _variants.FirstOrDefault(v => !v.IsDeleted && !v.Attributes.Any());

    // Specifications
    // Key = The "Key" from Category SpecDefinition (e.g., "screen_size")
    // Value = The actual value (e.g., "27 inches")
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
        string name,
        string baseSku,
        string? description,
        ProductCategory category,
        Dictionary<string, string> specifications,
        Money baseCost,
        Money basePrice,
        Weight? baseWeight,
        List<ProductOption>? options = null,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(Error.Validation("Product.NameRequired", "Product name is required."));

        if (category.Id == Guid.Empty)
            return Result.Failure<Product>(Error.Validation("Product.CategoryRequired", "Category is required."));

        bool hasVariants = options != null && options.Count > 0;

        if (hasVariants && options!.Count > 3)
            return Result.Failure<Product>(Error.Validation("Product.TooManyOptions", "A product can have a maximum of 3 options."));

        var product = new Product(
            Guid.NewGuid(),
            name,
            baseSku,
            description,
            category.Id,
            baseCost,
            basePrice,
            baseWeight,
            hasVariants);

        var specResult = product.UpdateSpecifications(specifications, category);
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

    public Result UpdateDetails(
        string name,
        string? description,
        Guid categoryId,
        Money baseCost,
        Money basePrice,
        Weight? baseWeight,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Product.NameRequired", "Product name is required."));

        if (categoryId == Guid.Empty)
            return Result.Failure(Error.Validation("Product.CategoryRequired", "Category is required."));

        Name = name;
        Description = description;
        CategoryId = categoryId;
        BaseCost = baseCost;
        BasePrice = basePrice;
        BaseWeight = baseWeight;

        _tags.Clear();
        if (tags != null && tags.Any())
        {
            _tags.AddRange(tags);
        }

        return Result.Success();
    }

    public Result UpdateSpecifications(Dictionary<string, string> newSpecs, ProductCategory category)
    {
        // Check for required fields
        foreach(var specDef in category.Specifications.Where(sp => sp.IsRequired))
        {
            if(!newSpecs.ContainsKey(specDef.Key) || string.IsNullOrWhiteSpace(newSpecs[specDef.Key]))
                return Result.Failure(Error.Validation("Product.MissingSpec", $"Specification '{specDef.Label}' is required."));
        }

        // Remove junk data
        // Only keep keys that actually exist in the category definition
        var validKeys = category.Specifications.Select(sp => sp.Key).ToHashSet();
        var cleanSpecs = newSpecs
            .Where(kvp => validKeys.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        _specifications = cleanSpecs;
        return Result.Success();
    }

    public Result UpdateOptionValues(List<ProductOption> newOptions, string userId = "System - Option value removed")
    {
        // Structural integrity check - cannot allow adding/removing entire axes
        bool axesMatch = _options.Count == newOptions.Count &&
            _options.All(o => newOptions.Any(n => n.Name == o.Name));

        if(!axesMatch && _variants.Any(v => !v.IsDeleted))
            return Result.Failure(Error.Validation("Product.StructureChanged",
                "You cannot add or remove entire Option categories (like removing 'Size') while variants exist. Delete all variants first."));

        // Value integrity check
        // If the user removed "Red", we must ensure "Red" variants are handled.

        // Get all allowed values from the NEW options
        // e.g. Color: [Blue, Green] (Red is missing)
        var allowedAttributes = newOptions
            .SelectMany(o => o.Values.Select(v => new { Option = o.Name, Value = v }))
            .ToList();

        // Check not deleted variants
        foreach( var variant in _variants.Where(v => !v.IsDeleted))
        {
            bool isValid = variant.Attributes.All(attr =>
                allowedAttributes.Any(allowed => allowed.Option == attr.Name && allowed.Value == attr.Value));

            if (!isValid)
                variant.Delete(userId);
                //return Result.Failure(Error.Validation("Product.VariantConflict",
                    //$"Cannot remove option value '{variant.DisplayName}' because active variants exist. Delete those variants first."));
        }

        _options.Clear();
        _options.AddRange(newOptions);

        return Result.Success();
    }

    public Result ChangeBaseSku(string newBaseSku)
    {
        if (string.IsNullOrEmpty(newBaseSku))
            return Result.Failure(Error.Validation("Product.SkuRequired", "Product SKU cannot be empty."));

        BaseSku = newBaseSku.ToUpperInvariant();
        return Result.Success();
    }

    public void Activate() => IsActive = true;
    public void Deactivate()
    {
        foreach (var variant in _variants)
            variant.Deactivate();

        IsActive = false;
    }

    // Methods to manage variants/images can be added here or handled via separate aggregates/repos if strict DDD is relaxed for performance.
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
}
