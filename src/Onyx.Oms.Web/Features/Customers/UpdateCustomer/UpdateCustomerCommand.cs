using Onyx.Oms.Core.Messaging;
using System.Text.Json.Serialization;

namespace Onyx.Oms.Web.Features.Customers.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? Street,
    string? City,
    string? District,
    string? State,
    string? PostalCode,
    string? Country,
    string? Notes,
    string? DeliveryInstructions) : ICommand;
