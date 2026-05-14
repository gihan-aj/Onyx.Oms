using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.CreateCustomer;

public class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCustomerHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        Guid? tenantId = _currentUserService.ActiveTenantId;
        if (tenantId == null)
            return Result.Failure<Guid>(Error.Unauthorized("Customer.TenantIdMissing", "Tenant Id not found."));

        // Check for unique email if provided
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Note: DB index also enforces this, but good to give friendly error
            bool isEmailTaken = await _context.Customers
                .AnyAsync(c => c.Email == request.Email, cancellationToken);
            
            if (isEmailTaken)
            {
                return Result.Failure<Guid>(Error.Conflict("Customer.EmailNotUnique", "A customer with this email already exists."));
            }
        }

        var address = new Address(
            request.Street ?? string.Empty,
            request.City ?? string.Empty,
            request.District ?? string.Empty,
            request.State ?? string.Empty,
            request.PostalCode ?? string.Empty,
            request.Country ?? string.Empty);

        var result = Customer.Create(
            tenantId.Value,
            request.Name,
            request.Email,
            request.PrimaryPhone,
            request.SecondaryPhone,
            address,
            request.Notes,
            request.DeliveryInstructions);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        _context.Customers.Add(result.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
