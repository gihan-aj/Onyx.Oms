using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerById;

public record CustomerDto(
    Guid Id,
    string Name,
    string? Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    Address Address,
    string? LastOrderNumber,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedDate);
