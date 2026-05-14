using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GenerateBulkShippingLabels
{
    public record GenerateBulkShippingLabelsQuery(List<Guid> OrderIds) : IQuery<byte[]>;
}
