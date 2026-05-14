using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
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
        var processingOrdersExists = await _context.Orders
            .AnyAsync(o => 
                o.CustomerId == customer.Id &&
                (o.Status == OrderStatus.Pending || 
                o.Status == OrderStatus.Confirmed || 
                o.Status == OrderStatus.Processing || 
                o.Status == OrderStatus.ReadyToPack ||
                o.Status == OrderStatus.Packed ||
                o.Status == OrderStatus.Shipped ||
                o.Status == OrderStatus.Delivered), 
                cancellationToken);
        if(processingOrdersExists)
        {
            return Result.Failure(Error.Validation("Customer.HasUnfinishedOrders", "Customer has unfinished orders and cannot be deactivated."));
        }

        customer.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
