using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.UpdateCustomer;

public class UpdateCustomerHandler : ICommandHandler<UpdateCustomerCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found."));
        }

        // Check for unique email if provided and changed
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
        {
            bool isEmailTaken = await _context.Customers
                .AnyAsync(c => c.Email == request.Email && c.Id != request.Id, cancellationToken);
            
            if (isEmailTaken)
            {
                return Result.Failure(Error.Conflict("Customer.EmailNotUnique", "A customer with this email already exists."));
            }
        }

        var address = new Address(
            request.Street ?? string.Empty,
            request.City ?? string.Empty,
            request.State ?? string.Empty,
            request.PostalCode ?? string.Empty,
            request.Country ?? string.Empty);

        customer.UpdateDetails(
            request.Name,
            request.Email,
            request.PrimaryPhone,
            request.SecondaryPhone,
            address,
            request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
