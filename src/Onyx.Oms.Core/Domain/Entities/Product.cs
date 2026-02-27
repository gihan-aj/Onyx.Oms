using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
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
        string? brand,
        string? material,
        Gender gender,
        Money baseCost,
        Money basePrice,
        Weight baseWeight) : base(id)
    {
        Name = name;
        BaseSku = baseSku;
        Description = description;
        CategoryId = categoryId;
        Brand = brand;
        Material = material;
        Gender = gender;
        BasePrice = basePrice;
        BaseCost = baseCost;
        BaseWeight = baseWeight;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string BaseSku { get; private set; } = string.Empty; // Can be auto-generated or set by user
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public string? Brand { get; private set; }
    public string? Material { get; private set; }
    public Gender Gender { get; private set; }

    // Financials & Measurements
    public Money BaseCost { get; private set; } = Money.Zero();
    public Money BasePrice { get; private set; } = Money.Zero();
    public Weight BaseWeight { get; private set; } = Weight.Zero();

    public bool IsActive { get; private set; }

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
        Guid categoryId,
        string? brand,
        string? material,
        Gender gender,
        Money baseCost,
        Money basePrice,
        Weight baseWeight,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(Error.Validation("Product.NameRequired", "Product name is required."));

        if (categoryId == Guid.Empty)
            return Result.Failure<Product>(Error.Validation("Product.CategoryRequired", "Category is required."));

        var product = new Product(
            Guid.NewGuid(),
            name,
            baseSku,
            description,
            categoryId,
            brand,
            material,
            gender,
            baseCost,
            basePrice,
            baseWeight);

        if (tags != null && tags.Any())
        {
            product._tags.AddRange(tags);
        }

        return Result.Success(product);
    }

    public void UpdateDetails(
        string name,
        string? description,
        Guid categoryId,
        string? brand,
        string? material,
        Gender gender,
        Money baseCost,
        Money basePrice,
        Weight baseWeight,
        List<string>? tags = null)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        Brand = brand;
        Material = material;
        Gender = gender;
        BaseCost = baseCost;
        BasePrice = basePrice;
        BaseWeight = baseWeight;

        _tags.Clear();
        if (tags != null && tags.Any())
        {
            _tags.AddRange(tags);
        }
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
    public void AddVariant(ProductVariant variant)
    {
        _variants.Add(variant);
    }

    public void AddImage(ProductImage image)
    {
        _images.Add(image);
    }
}
