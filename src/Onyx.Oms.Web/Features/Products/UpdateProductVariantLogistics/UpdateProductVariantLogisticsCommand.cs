using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Products.CreateProduct;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariantLogistics
{
    public record UpdateProductVariantLogisticsCommand(
        Guid ProductId,
        Guid VariantId,
        MoneyDto? Cost,
        MoneyDto? Price,
        WeightDto? Weight,
        int StockOnHand
    ) : ICommand;
}
