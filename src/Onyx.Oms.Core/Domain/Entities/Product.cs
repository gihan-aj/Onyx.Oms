using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class Product : AuditableEntity<Guid>
{
    private Product() { }

    internal Product(
        Guid id,
        string name,
        string? description,
        Guid categoryId,
        string? brand,
        string? material,
        Gender gender,
        decimal basePrice,
        decimal baseCost,
        decimal? baseWeight) : base(id)
    {
        Name = name;
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
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public string? Brand { get; private set; }
    public string? Material { get; private set; }
    public Gender Gender { get; private set; }

    // Financials (Base/Defaults)
    public decimal BasePrice { get; private set; }
    public decimal BaseCost { get; private set; }
    public decimal? BaseWeight { get; private set; }

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
        Guid categoryId,
        decimal basePrice,
        decimal baseCost,
        string? description = null,
        string? brand = null,
        string? material = null,
        Gender gender = Gender.Unisex,
        decimal? baseWeight = null,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(Error.Validation("Product.NameRequired", "Product name is required."));

        if (categoryId == Guid.Empty)
            return Result.Failure<Product>(Error.Validation("Product.CategoryRequired", "Category is required."));

        if (basePrice < 0)
            return Result.Failure<Product>(Error.Validation("Product.InvalidBasePrice", "Base price cannot be negative."));

        if (baseCost < 0)
            return Result.Failure<Product>(Error.Validation("Product.InvalidBaseCost", "Base cost cannot be negative."));

        var product = new Product(
            Guid.NewGuid(),
            name,
            description,
            categoryId,
            brand,
            material,
            gender,
            basePrice,
            baseCost,
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
        decimal basePrice,
        decimal baseCost,
        decimal? baseWeight)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        Brand = brand;
        Material = material;
        Gender = gender;
        BasePrice = basePrice;
        BaseCost = baseCost;
        BaseWeight = baseWeight;
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
