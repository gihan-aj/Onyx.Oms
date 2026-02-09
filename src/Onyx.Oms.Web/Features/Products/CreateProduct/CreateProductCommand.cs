using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    Guid CategoryId,
    decimal BasePrice,
    decimal BaseCost,
    string? Description,
    string? Brand,
    string? Material,
    Gender Gender,
    decimal? BaseWeight,
    List<string> Tags,
    List<CreateProductVariantDto> Variants,
    List<CreateProductImageDto> Images) : ICommand<Guid>;

public record CreateProductVariantDto(
    string Sku,
    string Name,
    string Size,
    string Color,
    decimal Price,
    decimal Cost,
    decimal? Weight,
    int StockOnHand);

public record CreateProductImageDto(
    string Url,
    int DisplayOrder,
    bool IsMain);
