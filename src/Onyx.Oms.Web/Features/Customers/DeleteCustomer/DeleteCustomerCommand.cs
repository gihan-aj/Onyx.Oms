using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : ICommand;
