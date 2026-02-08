using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto>;
