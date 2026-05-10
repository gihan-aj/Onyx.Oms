using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.GetOrderStatusCounts
{
    public class GetOrderStatusCountsHandler : IQueryHandler<GetOrderStatusCountsQuery, GetOrderStatusCountsResponse>
    {
        private readonly IApplicationDbContext _context;

        public GetOrderStatusCountsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<GetOrderStatusCountsResponse>> Handle(GetOrderStatusCountsQuery request, CancellationToken cancellationToken)
        {
            var query = from o in _context.Orders.AsNoTracking()
                        join c in _context.Customers.AsNoTracking() on o.CustomerId equals c.Id
                        select new { Order = o, Customer = c };

            // Filtering (excluding Statuses because we want all counts)
            if (request.PaymentStatus.HasValue)
            {
                query = query.Where(x => x.Order.PaymentStatus == request.PaymentStatus.Value);
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(x => x.Order.CustomerId == request.CustomerId.Value);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(x => x.Order.OrderDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(x => x.Order.OrderDate <= request.ToDate.Value);
            }

            if (request.IsCashOnDelivery.HasValue)
            {
                query = query.Where(x => x.Order.IsCashOnDelivery == request.IsCashOnDelivery.Value);
            }

            if (request.CourierId.HasValue)
            {
                query = query.Where(x => x.Order.CourierId == request.CourierId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(x =>
                    x.Order.OrderNumber.Contains(request.SearchTerm) ||
                    x.Customer.Name.Contains(request.SearchTerm) ||
                    (x.Customer.Email != null && x.Customer.Email.Contains(request.SearchTerm)) ||
                    (x.Customer.PrimaryPhone != null && x.Customer.PrimaryPhone.Contains(request.SearchTerm)) ||
                    (x.Order.TrackingNumber != null && x.Order.TrackingNumber.Contains(request.SearchTerm))
                );
            }

            // Group by status and count
            var counts = await query
                .GroupBy(x => x.Order.Status)
                .Select(g => new OrderStatusCountDto(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            var totalCount = counts.Sum(x => x.Count);

            return Result.Success(new GetOrderStatusCountsResponse(counts, totalCount));
        }
    }
}
