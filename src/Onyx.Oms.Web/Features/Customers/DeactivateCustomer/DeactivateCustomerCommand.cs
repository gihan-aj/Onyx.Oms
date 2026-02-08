using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.DeactivateCustomer;

public record DeactivateCustomerCommand(Guid Id) : ICommand;
