using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Customers.ActivateCustomer;

public class ActivateCustomerHandler : IRequestHandler<ActivateCustomerCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateCustomerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found."));
        }

        if (customer.IsActive)
        {
           return Result.Success();
        }

        customer.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
