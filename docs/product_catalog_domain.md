# Product Catalog Architecture & Business Logic - Onyx.Oms

## 1. Domain Architecture (Backend)

The Product Catalog is designed using Domain-Driven Design (DDD). The `Product` acts as the Aggregate Root, controlling the lifecycle of `ProductVariant` and `ProductImage` entities.



### 1.1 Value Objects & Primitive Obsession
To ensure type safety and mathematically safe operations, financials and measurements use Value Objects (`Money` and `Weight`). 
* **Validation:** Value Objects throw `ArgumentException` on invalid instantiation (e.g., negative amounts). The Application Layer (Commands/FluentValidation) is responsible for catching bad UI data *before* attempting to instantiate these objects.

### 1.2 Global Unit Handling (Tenant Configuration)
Units and currencies are strictly controlled to prevent data corruption between the UI and Database.
* **Database/API:** The system uses a `TenantConfiguration` or `StoreSettings` table to define the global `BaseCurrency` (e.g., "LKR") and `WeightUnit` (e.g., "kg").
* **API Contracts:** The API *must* receive both the value and the unit from the client. The Command Handler validates that the incoming unit matches the Tenant Configuration before saving.

### 1.3 Core Domain Rules
* **The "Copy on Creation" Pricing Rule:** Variants do not use nullable fallbacks for prices/weights. If a variant-specific price isn't provided, the Application Layer passes the `Product.BasePrice` into the variant factory. The database row always holds concrete values.
* **Variant Naming (Computed):** Variants do not store a `Name` column in the database. `DisplayName` is a computed property (`{Product.Name} - {Color} - {Size}`) to prevent update anomalies.
* **Strict Physical Reality:** `ReserveStock` cannot push `AvailableQuantity` below zero. It allocates what it physically can and returns the `unfulfilledQuantity`. The Application layer uses this remainder to generate `FulfillmentTasks` (Production/Procurement).
* **Color-Tagging Images:** Images are linked to the `Product` but tagged with a `Color` string. This prevents the user from uploading the exact same image for every size of a specific color.
* **Flexible Variant Attributes:** Not all clothing items have both Size and Color (e.g., Scarves only have Color, Socks may only have Size).
    * The `Product` entity contains flags (`HasColor`, `HasSize`) to indicate which attributes are active.
    * The `ProductVariant` entity allows `Color` and `Size` to be null.
    * **Validation:** The `Create` method enforces that if `Product.HasColor` is true, the Variant must provide a Color.

### 1.4 Final Domain Entities
```csharp
// Value Objects
public record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency = "LKR") => new(0, currency);
}
public record Weight(decimal Value, string Unit)
{
    public static Weight Zero(string unit = "kg") => new(0, unit);
}

// Entities
public class Product : AuditableEntity<Guid>
{
    private Product() { }

    internal Product(Guid id, string name, string baseSku, string? description, Guid categoryId, string? brand, string? material, Gender gender, Money baseCost, Money basePrice, Weight baseWeight) : base(id)
    {
        Name = name; BaseSku = baseSku; Description = description; CategoryId = categoryId;
        Brand = brand; Material = material; Gender = gender;
        BasePrice = basePrice; BaseCost = baseCost; BaseWeight = baseWeight; IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string BaseSku { get; private set; } = string.Empty; 
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public string? Brand { get; private set; }
    public string? Material { get; private set; }
    public Gender Gender { get; private set; }

    public Money BaseCost { get; private set; } = Money.Zero();
    public Money BasePrice { get; private set; } = Money.Zero();
    public Weight BaseWeight { get; private set; } = Weight.Zero();
    public bool IsActive { get; private set; }

    public virtual ProductCategory Category { get; private set; } = null!;
    
    private readonly List<ProductVariant> _variants = new();
    public virtual IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    
    private readonly List<ProductImage> _images = new();
    public virtual IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    
    private readonly List<string> _tags = new();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public static Result<Product> Create(string name, string baseSku, string? description, Guid categoryId, string? brand, string? material, Gender gender, Money baseCost, Money basePrice, Weight baseWeight, List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure<Product>(Error.Validation("Product.NameRequired", "Product name is required."));
        if (categoryId == Guid.Empty) return Result.Failure<Product>(Error.Validation("Product.CategoryRequired", "Category is required."));

        var product = new Product(Guid.NewGuid(), name, baseSku, description, categoryId, brand, material, gender, baseCost, basePrice, baseWeight);
        if (tags != null && tags.Any()) product._tags.AddRange(tags);
        return Result.Success(product);
    }

    public void UpdateDetails(string name, string? description, Guid categoryId, string? brand, string? material, Gender gender, Money baseCost, Money basePrice, Weight baseWeight, List<string>? tags = null)
    {
        Name = name; Description = description; CategoryId = categoryId; Brand = brand; Material = material; Gender = gender;
        BaseCost = baseCost; BasePrice = basePrice; BaseWeight = baseWeight;
        _tags.Clear();
        if (tags != null && tags.Any()) _tags.AddRange(tags);
    }

    public Result ChangeBaseSku(string newBaseSku)
    {
        if (string.IsNullOrEmpty(newBaseSku)) return Result.Failure(Error.Validation("Product.SkuRequired", "Product SKU cannot be empty."));
        BaseSku = newBaseSku.ToUpperInvariant();
        return Result.Success();
    }

    public void Activate() => IsActive = true;
    public void Deactivate()
    {
        foreach (var variant in _variants) variant.Deactivate();
        IsActive = false;
    }

    public void AddVariant(ProductVariant variant) => _variants.Add(variant);
    public void AddImage(ProductImage image) => _images.Add(image);
}

public class ProductVariant : AuditableEntity<Guid>
{
    private ProductVariant() { }

    internal ProductVariant(Guid id, Guid productId, string sku, string color, string size, Money cost, Money price, Weight weight, int stockOnHand) : base(id)
    {
        ProductId = productId; Sku = sku; Color = color; Size = size;
        Cost = cost; Price = price; Weight = weight;
        StockOnHand = stockOnHand; ReservedQuantity = 0; IsActive = true;
    }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;

    // Nullable Attributes
    public string? Color { get; private set; }
    public string? Size { get; private set; }

    // Smart Display Name
    public string DisplayName 
    {
        get 
        {
            var parts = new List<string> { Product?.Name ?? "Unknown" };
            if (!string.IsNullOrEmpty(Color)) parts.Add(Color);
            if (!string.IsNullOrEmpty(Size)) parts.Add(Size);
            return string.Join(" - ", parts);
        }
    }

    public Money Cost { get; private set; } = Money.Zero();
    public Money Price { get; private set; } = Money.Zero();
    public Weight Weight { get; private set; } = Weight.Zero();

    public int StockOnHand { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockOnHand - ReservedQuantity; 
    public bool IsActive { get; private set; }

    public virtual Product Product { get; private set; } = null!;

    public static Result<ProductVariant> Create(Guid productId, string sku, string? color, string? size, Money baseCost, Money basePrice, Weight baseWeight, Money? variantCost, Money? variantPrice, Weight? variantWeight, int stockOnHand = 0)
    {
        if (string.IsNullOrWhiteSpace(sku)) return Result.Failure<ProductVariant>(Error.Validation("ProductVariant.SkuRequired", "SKU is required."));

        // Validation logic depends on the Parent Product flags
        // This is usually handled in the Command Handler which loads the Parent Product first

        var variant = new ProductVariant(Guid.NewGuid(), productId, sku, color, size, variantCost ?? baseCost, variantPrice ?? basePrice, variantWeight ?? baseWeight, stockOnHand);
        return Result.Success(variant);
    }

    public Result UpdateDetails(string color, string size, Money baseCost, Money basePrice, Weight baseWeight, Money? variantCost, Money? variantPrice, Weight? variantWeight)
    {
        if (string.IsNullOrWhiteSpace(size)) return Result.Failure(Error.Validation("ProductVariant.SizeRequired", "Size is required."));
        if (string.IsNullOrWhiteSpace(color)) return Result.Failure(Error.Validation("ProductVariant.ColorRequired", "Color is required."));

        Color = color; Size = size;
        Cost = variantCost ?? baseCost; Price = variantPrice ?? basePrice; Weight = variantWeight ?? baseWeight;
        return Result.Success();
    }

    public Result ChangeSku(string newSku)
    {
        if(string.IsNullOrWhiteSpace(newSku)) return Result.Failure(Error.Validation("ProductVariant.SkuRequired", "SKU cannot be empty."));
        Sku = newSku.ToUpperInvariant();
        return Result.Success();
    }

    public void AdjustStock(int quantityAdjustment) => StockOnHand += quantityAdjustment;

    public Result<int> ReserveStock(int requestedQuantity)
    {
        int allocatableQuantity = Math.Min(requestedQuantity, AvailableQuantity);
        ReservedQuantity += allocatableQuantity;
        return Result.Success(requestedQuantity - allocatableQuantity);
    }

    public void ReleaseReservation(int quantity)
    {
        ReservedQuantity -= quantity;
        if (ReservedQuantity < 0) ReservedQuantity = 0;
    }

    public void MarkPacked(int quantity)
    {
        StockOnHand -= quantity;
        ReservedQuantity -= quantity;
        if (ReservedQuantity < 0) ReservedQuantity = 0;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public class ProductImage : Entity<Guid>
{
    public ProductImage(Guid id, Guid productId, string url, int displayOrder, bool isMain) : base(id)
    {
        ProductId = productId; Url = url; DisplayOrder = displayOrder; IsMain = isMain;
    }

    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }
    public string? Color { get; private set; } 

    public virtual Product Product { get; private set; } = null!;

    public Result TagWithColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return Result.Failure(Error.Validation("ProductImage.EmptyColorName", "Color is required."));
        Color = color;
        return Result.Success();
    }

    public void RemoveColorTag() => Color = null;
}
```



## 2. Entity Framework Core Configuration
Uses `.ComplexProperty` to flatten Value Objects into standard database columns.

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.BaseSku).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Tags).HasColumnName("Tags");

        builder.ComplexProperty(p => p.BasePrice, pb => {
            pb.Property(m => m.Amount).HasColumnName("BasePriceAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("BasePriceCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(p => p.BaseCost, cb => {
            cb.Property(m => m.Amount).HasColumnName("BaseCostAmount").HasPrecision(18, 2);
            cb.Property(m => m.Currency).HasColumnName("BaseCostCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(p => p.BaseWeight, wb => {
            wb.Property(w => w.Value).HasColumnName("BaseWeightValue").HasPrecision(10, 3);
            wb.Property(w => w.Unit).HasColumnName("BaseWeightUnit").HasMaxLength(10);
        });

        builder.HasMany(p => p.Variants).WithOne(v => v.Product).HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => v.Sku).IsUnique();

        builder.Ignore(v => v.DisplayName);
        builder.Ignore(v => v.AvailableQuantity);

        builder.ComplexProperty(v => v.Price, pb => {
            pb.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
            pb.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(v => v.Cost, cb => {
            cb.Property(m => m.Amount).HasColumnName("CostAmount").HasPrecision(18, 2);
            cb.Property(m => m.Currency).HasColumnName("CostCurrency").HasMaxLength(3);
        });

        builder.ComplexProperty(v => v.Weight, wb => {
            wb.Property(w => w.Value).HasColumnName("WeightValue").HasPrecision(10, 3);
            wb.Property(w => w.Unit).HasColumnName("WeightUnit").HasMaxLength(10);
        });
    }
}
```

## 3. Client Architecture & UX Constraints (WinUI 3)

### 3.1 Task-Based UI vs. Monolithic Saves
The UI must avoid monolithic "Edit Product" pages. Destructive or high-risk actions (like changing a `BaseSku`) must be handled via specific, isolated interactions (e.g., a `ContentDialog` that explicitly asks the user if they want to cascade the SKU change to existing variants, warning them about barcode reprints).

### 3.2 Implicit Unit Handling
Users must never manually type units ("LKR" or "kg") during data entry. 
* The WinUI app fetches global unit settings on startup.
* TextBoxes only accept decimals. 
* The ViewModel automatically appends the global unit string to the decimal value to construct the DTO payload sent to the API.



### 3.3 The Variant Matrix (Bulk Creation)
To prevent tedious data entry, the system uses a Matrix Generator instead of static database lookup tables for sizes and colors.
1. The UI queries distinct colors and sizes currently in the database to populate a list of Checkboxes.
2. The user checks desired colors (e.g., Red, Blue) and sizes (e.g., M, L).
3. The ViewModel computes a Cartesian product, generating draft variants in an `ObservableCollection`.
4. The user views these in a `DataGrid`, making specific price/SKU overrides before saving all at once.

### 3.4 State Services
Complex, multi-step actions (like building a draft order or a massive product matrix) rely on an injected `StateService` (Observable Singleton) to hold state in-memory without polluting navigation parameters or prematurely hitting the database.

### 3.5 The "Quick-Add" Matrix (Order Entry)
To speed up order creation, the UI replaces standard dropdowns with a **Quantity Matrix**:
* **If Product has Color & Size:** Displays a grid (Rows=Colors, Columns=Sizes). User enters quantities in cells (e.g., Red/M = 5).
* **If Product has Size Only:** Displays a simple list of Sizes with Quantity inputs.
* **If Product has Color Only:** Displays a simple list of Colors with Quantity inputs.
* **Action:** A single "Add to Order" button processes the non-zero inputs and creates multiple `OrderItem` entries in one batch.