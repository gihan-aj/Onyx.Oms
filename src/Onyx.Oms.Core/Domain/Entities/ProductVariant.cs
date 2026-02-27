using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class ProductVariant : AuditableEntity<Guid>
{
    private ProductVariant() { }

    internal ProductVariant(
        Guid id,
        Guid productId,
        string sku,
        string color,
        string size,
        Money cost,
        Money price,
        Weight weight,
        int stockOnHand) : base(id)
    {
        ProductId = productId;
        Sku = sku;
        Color = color;
        Size = size;
        Cost = cost;
        Price = price;
        Weight = weight;
        StockOnHand = stockOnHand;
        ReservedQuantity = 0;
        IsActive = true;
    }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public string Size { get; private set; } = string.Empty;
    public string DisplayName
    {
        get
        {
            string baseName = Product?.Name ?? "Unknown Product";
            return $"{baseName} - {Color} - {Size}";
        }
    }

    // Overrides
    public Money Cost { get; private set; } = Money.Zero();
    public Money Price { get; private set; } = Money.Zero();
    public Weight Weight { get; private set; } = Weight.Zero();

    // Inventory
    public int StockOnHand { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockOnHand - ReservedQuantity; // Computed property

    public bool IsActive { get; private set; }

    // Navigation
    public virtual Product Product { get; private set; } = null!;

    public static Result<ProductVariant> Create(
        Guid productId,
        string sku,
        string color,
        string size,
        Money baseCost,
        Money basePrice,
        Weight baseWeight,
        Money? variantCost,
        Money? variantPrice,
        Weight? variantWeight,
        int stockOnHand = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SkuRequired", "SKU is required."));

        if (string.IsNullOrWhiteSpace(size))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SizeRequired", "Size is required."));

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.ColorRequired", "Color is required."));

        var variant = new ProductVariant(
            Guid.NewGuid(),
            productId,
            sku,
            color,
            size,
            variantCost ?? baseCost,
            variantPrice ?? basePrice,
            variantWeight ?? baseWeight,
            stockOnHand);

        return Result.Success(variant);
    }

    public Result UpdateDetails(
        string color, 
        string size,
        Money baseCost,
        Money basePrice,
        Weight baseWeight,
        Money? variantCost, 
        Money? variantPrice, 
        Weight? variantWeight)
    {
        if (string.IsNullOrWhiteSpace(size))
            return Result.Failure(Error.Validation("ProductVariant.SizeRequired", "Size is required."));

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure(Error.Validation("ProductVariant.ColorRequired", "Color is required."));


        Color = color;
        Size = size;
        Cost = variantCost ?? baseCost;
        Price = variantPrice ?? basePrice;
        Weight = variantWeight ?? baseWeight;

        return Result.Success();
    }

    public Result ChangeSku(string newSku)
    {
        if(string.IsNullOrWhiteSpace(newSku))
            return Result.Failure(Error.Validation("ProductVariant.SkuRequired", "SKU cannot be empty."));

        Sku = newSku;
        return Result.Success();
    }

    public void AdjustStock(int quantityAdjustment)
    {
        StockOnHand += quantityAdjustment;
    }

    public Result<int> ReserveStock(int requestedQuantity)
    {
        int allocatableQuantity = Math.Min(requestedQuantity, AvailableQuantity);

        ReservedQuantity += allocatableQuantity;

        int unfulfilledQuantity = requestedQuantity - allocatableQuantity;

        return Result.Success(unfulfilledQuantity);
    }

    public void ReleaseReservation(int quantity)
    {
        ReservedQuantity -= quantity;
        if (ReservedQuantity < 0) ReservedQuantity = 0;
    }

    public void MarkPacked(int quantity)
    {
        // When packed, we remove from Stock AND remove from Reserved.
        StockOnHand -= quantity;
        ReservedQuantity -= quantity;
        if (ReservedQuantity < 0) ReservedQuantity = 0;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
