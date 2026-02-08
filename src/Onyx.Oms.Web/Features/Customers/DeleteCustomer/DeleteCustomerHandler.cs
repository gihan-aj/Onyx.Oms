using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Customers.DeleteCustomer;

public class DeleteCustomerHandler : IRequestHandler<DeleteCustomerCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteCustomerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        // CHECK FOR ORDERS LATER....

        if (customer is null)
        {
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found."));
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
