using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.DeactivateCustomer;

public class DeactivateCustomerHandler : ICommandHandler<DeactivateCustomerCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateCustomerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found."));
        }

        if (!customer.IsActive)
        {
           return Result.Success();
        }

        // CHECK FOR UNFINISHED ORDERS LATER....

        customer.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
