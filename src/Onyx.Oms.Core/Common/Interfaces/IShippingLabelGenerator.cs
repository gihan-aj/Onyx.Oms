using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface IShippingLabelGenerator
    {
        byte[] Generate(Order order, Customer customer, Tenant tenant);
        byte[] GenerateBulk(List<Order> orders, Dictionary<Guid, Customer> customers, Tenant tenant);
    }
}
