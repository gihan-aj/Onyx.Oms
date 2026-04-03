using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Web.Features.Customers.GetCustomersPaged;

public record CustomerDto(
    Guid Id,
    string Name,
    string? Email,
    string PrimaryPhone,
    string? SecondaryPhone,
    string? LastOrderNumber,
    Address Address,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedDate);
