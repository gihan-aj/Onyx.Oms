using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class ProductVariant : AuditableEntity<Guid>, ISoftDeletable, IMustHaveTenant
{
    private ProductVariant() : base(Guid.NewGuid()) { }

    internal ProductVariant(
        Guid tenantId,
        Guid productId,
        string sku,
        List<VariantAttribute> attributes,
        Money cost,
        Money price,
        Weight? weight,
        int stockOnHand) : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        ProductId = productId;
        Sku = sku;
        Cost = cost;
        Price = price;
        Weight = weight;
        StockOnHand = stockOnHand;
        ReservedQuantity = 0;
        IsActive = true;

        _attributes.AddRange(attributes);
    }

    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;

    // Dynamic attributes
    // e.g. [{ "Name": "Color", "Value": "Red" }, { "Name": "Size", "Value": "Large" }]
    private readonly List<VariantAttribute> _attributes = new();
    public IReadOnlyCollection<VariantAttribute> Attributes => _attributes.AsReadOnly();
        
    public string DisplayName
    {
        get
        {
            var parts = new List<string> { Product?.Name ?? "Unknown Product" };
            if(_attributes.Count > 0)
            {
                foreach (var attr in _attributes)
                {
                    parts.Add(attr.Value);
                }
                return string.Join(" · ", parts);
            }
            return parts[0];
        }
    }

    // Overrides
    public Money Cost { get; private set; } = Money.Zero();
    public Money Price { get; private set; } = Money.Zero();
    public Weight? Weight { get; private set; }

    // Inventory
    public int StockOnHand { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int IncomingStock {  get; private set; }
    public int AvailableQuantity => StockOnHand - ReservedQuantity; // Computed property

    public bool IsActive { get; private set; }

    public DateTimeOffset? OutOfStockSinceUtc { get; private set; }

    public bool IsDeleted => DeletedAtUtc is not null;
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? DeletedBy { get; private set; }

    // Navigation
    public virtual Product Product { get; private set; } = null!;

    public static Result<ProductVariant> Create(
        Guid tenantId,
        Product product,
        string sku,
        List<VariantAttribute> attributes,
        Money? variantCost,
        Money? variantPrice,
        Weight? variantWeight,
        int stockOnHand = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SkuRequired", "SKU is required."));

        if (!product.HasVariants)
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.NotAllowed", "This product does not use variants. Use Product.SetDefaultVariantLogistics() instead."));

        // Validate Attribute Count
        if (attributes.Count != product.Options.Count)
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.AttributeMismatch", $"This product requires exactly {product.Options.Count} options."));

        // Validate Attribute Names and Values
        foreach(var option in product.Options)
        {
            var matchingAttr = attributes.FirstOrDefault(a => a.Name.Equals(option.Name, StringComparison.OrdinalIgnoreCase));

            if(matchingAttr == null)
                return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.MissingOption", $"Missing value for option '{option.Name}'."));

            if (!option.Values.Contains(matchingAttr.Value))
                return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.InvalidValue", $"'{matchingAttr.Value}' is not a valid value for option '{option.Name}'. Allowed: {string.Join(", ", option.Values)}"));
        }

        var variant = new ProductVariant(
            tenantId,
            product.Id,
            sku,
            attributes,
            variantCost ?? product.BaseCost,
            variantPrice ?? product.BasePrice,
            variantWeight ?? product.BaseWeight,
            stockOnHand);

        return Result.Success(variant);
    }

    /// <summary>
    /// Creates the internal default variant for a variant-less product.
    /// No attribute validation is performed — this variant intentionally has no attributes.
    /// </summary>
    internal static Result<ProductVariant> CreateDefault(
        Guid tenantId,
        Product product,
        string sku,
        Money cost,
        Money price,
        Weight? weight,
        int stockOnHand = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SkuRequired", "SKU is required for the default variant."));

        var variant = new ProductVariant(
            tenantId,
            product.Id,
            sku,
            new List<VariantAttribute>(), // empty — no selectable options
            cost,
            price,
            weight,
            stockOnHand);

        return Result.Success(variant);
    }

    public Result UpdateLogistics(
        Money baseCost,
        Money basePrice,
        Weight? baseWeight,
        Money? variantCost, 
        Money? variantPrice, 
        Weight? variantWeight)
    {
        if (IsDeleted)
            return Result.Failure(Error.Validation("Variant.Deleted", "Cannot update a deleted variant."));

        Cost = variantCost ?? baseCost;
        Price = variantPrice ?? basePrice;
        Weight = variantWeight ?? baseWeight;

        return Result.Success();
    }

    /// <summary>
    /// Updates logistics for the default (no-attribute) variant of a variant-less product.
    /// Called internally via Product.SetDefaultVariantLogistics().
    /// </summary>
    internal Result UpdateDefaultLogistics(
        string sku,
        Money cost,
        Money price,
        Weight? weight,
        int stockOnHand)
    {
        if (IsDeleted)
            return Result.Failure(Error.Validation("Variant.Deleted", "Cannot update a deleted variant."));

        Sku = sku;
        Cost = cost;
        Price = price;
        Weight = weight;
        StockOnHand = stockOnHand;

        return Result.Success();
    }

    public Result ChangeSku(string newSku)
    {
        if(string.IsNullOrWhiteSpace(newSku))
            return Result.Failure(Error.Validation("ProductVariant.SkuRequired", "SKU cannot be empty."));

        Sku = newSku;
        return Result.Success();
    }

    public Result AdjustStock(int quantityAdjustment)
    {
        StockOnHand += quantityAdjustment;
        if (StockOnHand < 0)
            return Result.Failure(Error.Validation("Stock.NegativeValue", "Stock on hand cannot be negative."));

        EvaluateStockStatus();
        return Result.Success();
    }

    public Result AdjustIncomingStock(int quantityAdjustment)
    {
        IncomingStock += quantityAdjustment;
        if (IncomingStock < 0)
            return Result.Failure(Error.Validation("IncomingStock.NegativeValue", "Incoming stock cannot be negative."));

        return Result.Success();
    }

    public Result<int> ReserveStock(int requestedQuantity)
    {
        if (requestedQuantity <= 0)
            return Result.Failure<int>(Error.Validation("ProductVariant.InvalidRequestQuantity", "Request to reserve quantity should be greater than zero."));

        int allocatableQuantity = Math.Min(requestedQuantity, AvailableQuantity);

        ReservedQuantity += allocatableQuantity;

        int unfulfilledQuantity = requestedQuantity - allocatableQuantity;

        EvaluateStockStatus();
        return Result.Success(unfulfilledQuantity);
    }

    public Result ReserveStockFromTask(int requestedQuantity)
    {
        if (requestedQuantity <= 0)
            return Result.Failure<int>(Error.Validation("ProductVariant.InvalidRequestQuantity", "Request to reserve quantity should be greater than zero."));

        if (requestedQuantity > StockOnHand)
            return Result.Failure<int>(Error.Validation("ProductVariant.InvalidRequestQuantity", "Request to reserve quantity should not be greater than stock on hand."));

        ReservedQuantity += requestedQuantity;

        return Result.Success();
    }

    public void ReleaseReservation(int quantity)
    {
        ReservedQuantity -= quantity;
        if (ReservedQuantity < 0) ReservedQuantity = 0;

        EvaluateStockStatus();
    }

    public void MarkShipped(int quantity)
    {
        // When shipped, we remove from Stock AND remove from Reserved.
        StockOnHand -= quantity;
        ReservedQuantity -= quantity;
        if (ReservedQuantity < 0) ReservedQuantity = 0;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void Delete(Guid userId)
    {
        if (IsDeleted) return;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        DeletedBy = userId;
    }

    private void EvaluateStockStatus()
    {
        if (AvailableQuantity <= 0 && OutOfStockSinceUtc == null)
            OutOfStockSinceUtc = DateTimeOffset.UtcNow;
        else if (AvailableQuantity > 0 && OutOfStockSinceUtc != null)
            OutOfStockSinceUtc = null;
    }
}
