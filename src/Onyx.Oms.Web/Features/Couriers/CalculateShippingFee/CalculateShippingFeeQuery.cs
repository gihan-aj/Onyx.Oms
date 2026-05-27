using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Couriers.CalculateShippingFee
{
    public record CalculateShippingFeeQuery(
        Guid CourierId,
        string District,
        decimal TotalWeightKg,
        decimal CodAmount) : IQuery<decimal>;
}