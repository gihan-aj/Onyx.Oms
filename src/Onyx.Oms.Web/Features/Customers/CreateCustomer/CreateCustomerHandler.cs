using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Web.Features.Customers.CreateCustomer;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
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
            request.State ?? string.Empty,
            request.PostalCode ?? string.Empty,
            request.Country ?? string.Empty);

        var result = Customer.Create(
            request.Name,
            request.Email,
            request.PrimaryPhone,
            request.SecondaryPhone,
            address,
            request.Notes);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        _context.Customers.Add(result.Value);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
