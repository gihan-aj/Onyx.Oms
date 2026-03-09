using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IQuery<ProductDetailsDto>;

public record ProductDetailsDto(
    Guid Id,
    string Name,
    string BaseSku,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    string CategoryPath,
    List<ProductSpecDto> Specifications,
    decimal BaseCostAmount,
    string BaseCostCurrency,
    decimal BasePriceAmount,
    string BasePriceCurrency,
    decimal? BaseWeightAmount,
    string? BaseWeightCurrency,
    bool HasVariants,
    int? StockOnHand,
    int? ReservedQuantity,
    List<string> Tags,
    List<ProductOptionDto> Options,
    List<ProductVariantDto> Variants,
    List<ProductImageDto> Images,
    bool IsActive
);

public record ProductSpecDto(
    string Key,
    string Label,
    string Value
);

public record ProductOptionDto(
    string Name, 
    int DispalyOrder, 
    List<string> Values);

public record ProductVariantDto(
    Guid Id,
    string Sku,
    List<VariantAttributeDto> Attributes,
    decimal CostAmount,
    string CostCurrency,
    decimal PriceAmount,
    string PriceCurrency,
    decimal? WeightAmount,
    string? WeightCurrency,
    int StockOnHand,
    int ReservedQuantity,
    bool IsActive
);

public record VariantAttributeDto(
    string Name, 
    string Value
);

public record ProductImageDto(
    Guid Id,
    string Url,
    int DisplayOrder,
    bool IsMain,
    string? OptionName = null,
    string? OptionValue = null
);
