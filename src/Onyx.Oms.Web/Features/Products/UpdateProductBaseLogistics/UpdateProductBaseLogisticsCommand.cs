using Onyx.Oms.Core.Messaging;
using Onyx.Oms.Web.Features.Products.CreateProduct; // for re-using MoneyDto and WeightDto

namespace Onyx.Oms.Web.Features.Products.UpdateProductBaseLogistics
{
    public record UpdateProductBaseLogisticsCommand(
        Guid Id,
        MoneyDto BaseCost,
        MoneyDto BasePrice,
        WeightDto? BaseWeight
    ) : ICommand;
}
