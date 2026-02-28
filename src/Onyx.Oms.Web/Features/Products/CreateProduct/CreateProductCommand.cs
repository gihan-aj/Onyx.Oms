using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public record ProductVariantDto(string Sku, string? Color, string? Size, decimal? CostAmount, decimal? PriceAmount, decimal? WeightValue, int StockOnHand);
public record ProductImageDto(string Url, int DisplayOrder, bool IsMain, string? Color);

public record CreateProductCommand(
    string Name,
    string? BaseSku,
    string? Description,
    Guid CategoryId,
    string? Brand,
    string? Material,
    Gender Gender, // Expected as int or string, easily parseable by JSON deserializer
    decimal BaseCostAmount,
    string BaseCostCurrency,
    decimal BasePriceAmount,
    string BasePriceCurrency,
    decimal BaseWeightValue,
    string BaseWeightUnit,
    bool HasColor,
    bool HasSize,
    List<string>? Tags,
    List<ProductVariantDto>? Variants,
    List<ProductImageDto>? Images) : ICommand<Guid>;
