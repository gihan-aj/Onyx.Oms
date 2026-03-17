using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Products.CreateProduct;

namespace Onyx.Oms.Web.Features.Products.UpdateDefaultVariantLogistics
{
    public record UpdateDefaultVariantLogisticsCommand(
        Guid ProductId,
        string Sku,
        MoneyDto Cost,
        MoneyDto Price,
        WeightDto? Weight,
        int StockOnHand
    ) : ICommand;
}
