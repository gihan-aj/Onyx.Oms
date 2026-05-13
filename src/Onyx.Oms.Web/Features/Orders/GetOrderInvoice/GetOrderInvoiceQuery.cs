using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GetOrderInvoice
{
    public record GetOrderInvoiceQuery(Guid OrderId, string LogoStoragePath) : IQuery<byte[]>;
}
