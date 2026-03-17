using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Products.CreateProduct;

namespace Onyx.Oms.Web.Features.Products.AddProductVariant
{
    public record AddProductVariantCommand(
        Guid ProductId,
        string? Sku,
        List<VariantAttributeDto> Attributes,
        MoneyDto? Cost,
        MoneyDto? Price,
        WeightDto? Weight,
        int StockOnHand
    ) : ICommand<Guid>;
}
