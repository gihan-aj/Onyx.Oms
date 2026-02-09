using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class ProductVariant : AuditableEntity
{
    private ProductVariant() { }

    internal ProductVariant(
        Guid id,
        Guid productId,
        string sku,
        string name,
        string size,
        string color,
        decimal price,
        decimal cost,
        decimal? weight,
        int stockOnHand) : base(id)
    {
        ProductId = productId;
        Sku = sku;
        Name = name;
        Size = size;
        Color = color;
        Price = price;
        Cost = cost;
        Weight = weight;
        StockOnHand = stockOnHand;
        ReservedQuantity = 0;
        IsActive = true;
    }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty; // e.g. "Medium - Red"
    public string Size { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;

    // Overrides
    public decimal Price { get; private set; }
    public decimal Cost { get; private set; }
    public decimal? Weight { get; private set; }

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
        string name,
        string size,
        string color,
        decimal price,
        decimal cost,
        decimal? weight = null,
        int stockOnHand = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SkuRequired", "SKU is required."));

        if (string.IsNullOrWhiteSpace(size))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SizeRequired", "Size is required."));

        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.ColorRequired", "Color is required."));

        if (price < 0)
            return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.InvalidPrice", "Price cannot be negative."));

        var variant = new ProductVariant(
            Guid.NewGuid(),
            productId,
            sku,
            name,
            size,
            color,
            price,
            cost,
            weight,
            stockOnHand);

        return Result.Success(variant);
    }

    public void UpdateDetails(string sku, string name, string size, string color, decimal price, decimal cost, decimal? weight)
    {
        Sku = sku;
        Name = name;
        Size = size;
        Color = color;
        Price = price;
        Cost = cost;
        Weight = weight;
    }

    public void AdjustStock(int quantityAdjustment)
    {
        StockOnHand += quantityAdjustment;
    }

    public Result ReserveStock(int quantity)
    {
        if (AvailableQuantity < quantity)
        {
             // For some businesses, they might want to allow reservation even if stock is 0 (backorder).
             // Given the user requirement: "if not in stock... manufacture it".
             // So we CAN reserve it, which pushes Available into negative? Or we just track "Status"?
             // User said: "if not items in stock we have to add products that needs to fulfill separatey".
             // So we can reserve.
             // But traditionally, "Reserved" implies "Held for Order".
        }
        ReservedQuantity += quantity;
        return Result.Success();
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
