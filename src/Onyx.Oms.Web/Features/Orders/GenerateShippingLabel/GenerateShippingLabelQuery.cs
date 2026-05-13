using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GenerateShippingLabel
{
    public record GenerateShippingLabelQuery(Guid OrderId) : IQuery<byte[]>;
}
