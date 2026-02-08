using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string? Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? Notes) : ICommand<Guid>;
