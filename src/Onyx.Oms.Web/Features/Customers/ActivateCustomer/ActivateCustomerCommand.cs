using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.ActivateCustomer;

public record ActivateCustomerCommand(Guid Id) : ICommand;
