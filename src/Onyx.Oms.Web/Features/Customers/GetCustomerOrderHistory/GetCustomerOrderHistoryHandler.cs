using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Customers.GetCustomerOrderHistory
{
    public class GetCustomerOrderHistoryHandler : IQueryHandler<GetCustomerOrderHistoryQuery, CustomerOrderHistoryResponse>
    {
        private readonly IApplicationDbContext _context;
        public GetCustomerOrderHistoryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Result<CustomerOrderHistoryResponse>> Handle(GetCustomerOrderHistoryQuery request, CancellationToken cancellationToken)
        {
            var baseQuery = _context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == request.CustomerId);

            var totalCount = await baseQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return Result.Success(new CustomerOrderHistoryResponse(0, new List<CustomerOrderSummaryDto>()));
            }

            var recentOrders = await baseQuery
                .OrderByDescending(o => o.OrderDate) // Sort newest first
                .Take(request.Top)
                .Select(o => new CustomerOrderSummaryDto(
                    o.Id,
                    o.OrderNumber,
                    o.OrderDate,
                    o.Status,
                    o.PaymentStatus,
                    o.GrandTotal.Amount,
                    o.GrandTotal.Currency,
                    o.GrandTotal.Amount - o.Payments.Sum(p => p.Amount.Amount)
                ))
                .ToListAsync(cancellationToken);

            return Result.Success(new CustomerOrderHistoryResponse(totalCount, recentOrders));
        }
    }
}
