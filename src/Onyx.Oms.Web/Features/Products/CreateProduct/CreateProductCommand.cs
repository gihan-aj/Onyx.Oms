using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? BaseSku, // Optional: Auto-generated if empty
    string? Description,
    Guid CategoryId,

    // Base Financials & Logistics
    MoneyDto BaseCost,
    MoneyDto BasePrice,
    WeightDto? BaseWeight,
    int? BaseStockOnHand, // Used only if the product has NO variants

    // Dynamic Options (e.g. "Size", "Color")
    List<ProductOptionDto> Options,

    // Specifications (e.g. "Screen Size": "27 inch")
    Dictionary<string, string> Specifications,

    // Variants
    List<CreateProductVariantDto> Variants,

    // Images
    List<CreateProductImageDto> Images,

    // Tags
    List<string> Tags
) : ICommand<Guid>;

public record CreateProductVariantDto(
    string? Sku, // Optional: Auto-generated if empty using BaseSku pattern
    List<VariantAttributeDto> Attributes,
    MoneyDto? Cost,   // Optional: Defaults to BaseCost if null
    MoneyDto? Price,  // Optional: Defaults to BasePrice if null
    WeightDto? Weight, // Optional: Defaults to BaseWeight if null
    int StockOnHand
);
public record CreateProductImageDto(
    string Url,
    int DisplayOrder,
    bool IsMain,
    // Optional: link image to a specific option value (e.g. "Color" = "Red")
    string? OptionName = null,
    string? OptionValue = null
);
public record MoneyDto(decimal Amount, string Currency = "LKR");
public record WeightDto(decimal Value, string Unit = "kg");
public record ProductOptionDto(string Name, List<string> Values);
public record VariantAttributeDto(string Name, string Value);
