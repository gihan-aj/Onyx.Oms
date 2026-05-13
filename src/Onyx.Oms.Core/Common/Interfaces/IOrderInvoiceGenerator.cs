using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface IOrderInvoiceGenerator
    {
        byte[] Generate(Order order, Customer customer, Tenant tenant, string logoStoragePath);
    }
}
