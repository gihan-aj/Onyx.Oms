using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Enums;

namespace Onyx.Oms.Web.Features.Orders.GetOrdersPaged
{
    public class GetOrdersPagedHandler : IRequestHandler<GetOrdersPagedQuery, Result<PagedResult<OrderSummaryDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetOrdersPagedHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(GetOrdersPagedQuery request, CancellationToken cancellationToken)
        {
            var query = from o in _context.Orders.AsNoTracking()
                        join c in _context.Customers.AsNoTracking() on o.CustomerId equals c.Id
                        select new { Order = o, Customer = c };

            // Filtering
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Order.Status == request.Status.Value);
            }

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

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(x =>
                    x.Order.OrderNumber.Contains(request.SearchTerm) ||
                    x.Customer.Name.Contains(request.SearchTerm) ||
                    (x.Customer.Email != null && x.Customer.Email.Contains(request.SearchTerm)) ||
                    (x.Order.TrackingNumber != null && x.Order.TrackingNumber.Contains(request.SearchTerm))
                );
            }

            // Sorting
            bool isDesc = request.SortOrder?.ToLower() == "desc";

            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = isDesc ? query.OrderByDescending(x => x.Order.CreatedOnUtc) : query.OrderBy(x => x.Order.CreatedOnUtc);
            }
            else
            {
                query = request.SortColumn.ToLower() switch
                {
                    "ordernumber" => isDesc ? query.OrderByDescending(x => x.Order.OrderNumber) : query.OrderBy(x => x.Order.OrderNumber),
                    "orderdate" => isDesc ? query.OrderByDescending(x => x.Order.OrderDate) : query.OrderBy(x => x.Order.OrderDate),
                    "customername" => isDesc ? query.OrderByDescending(x => x.Customer.Name) : query.OrderBy(x => x.Customer.Name),
                    "grandtotal" => isDesc ? query.OrderByDescending(x => x.Order.GrandTotal.Amount) : query.OrderBy(x => x.Order.GrandTotal.Amount),
                    "status" => isDesc ? query.OrderByDescending(x => x.Order.Status) : query.OrderBy(x => x.Order.Status),
                    "paymentstatus" => isDesc ? query.OrderByDescending(x => x.Order.PaymentStatus) : query.OrderBy(x => x.Order.PaymentStatus),
                    "createddate" => isDesc ? query.OrderByDescending(x => x.Order.CreatedOnUtc) : query.OrderBy(x => x.Order.CreatedOnUtc),
                    _ => isDesc ? query.OrderByDescending(x => x.Order.CreatedOnUtc) : query.OrderBy(x => x.Order.CreatedOnUtc)
                };
            }

            // Projections
            IQueryable<OrderSummaryDto> dtoQuery;
            if (request.IncludeDetails.HasValue && request.IncludeDetails.Value)
            {
                dtoQuery = query.Select(x => new OrderSummaryDto(
                    x.Order.Id,
                    x.Order.OrderNumber,
                    x.Order.OrderDate,
                    x.Order.CustomerId,
                    x.Customer.Name,
                    x.Customer.Email,
                    x.Customer.PrimaryPhone,
                    x.Order.Status,
                    x.Order.PaymentStatus,
                    x.Order.GrandTotal.Amount,
                    x.Order.GrandTotal.Currency,
                    x.Order.Payments.Sum(p => p.Amount.Amount),
                    x.Order.GrandTotal.Amount - x.Order.Payments.Sum(p => p.Amount.Amount),
                    x.Order.IsCashOnDelivery,
                    x.Order.TrackingNumber,
                    x.Order.Items.Select(i => new OrderItemSummaryDto(
                        i.Id,
                        i.ProductVariantId,
                        i.Quantity,
                        i.UnitPrice.Amount,
                        i.UnitPrice.Currency,
                        i.LineTotal.Amount,
                        i.Status)).ToList(),
                    x.Order.Payments.Select(p => new OrderPaymentSummaryDto(
                        p.Id,
                        p.Amount.Amount,
                        p.Amount.Currency,
                        p.Method,
                        p.Reference,
                        p.PaymentDate)).ToList(),
                    x.Order.CreatedOnUtc,
                    x.Order.LastModifiedOnUtc
                ));
            }
            else
            {
                dtoQuery = query.Select(x => new OrderSummaryDto(
                    x.Order.Id,
                    x.Order.OrderNumber,
                    x.Order.OrderDate,
                    x.Order.CustomerId,
                    x.Customer.Name,
                    x.Customer.Email,
                    x.Customer.PrimaryPhone,
                    x.Order.Status,
                    x.Order.PaymentStatus,
                    x.Order.GrandTotal.Amount,
                    x.Order.GrandTotal.Currency,
                    x.Order.Payments.Sum(p => p.Amount.Amount),
                    x.Order.GrandTotal.Amount - x.Order.Payments.Sum(p => p.Amount.Amount),
                    x.Order.IsCashOnDelivery,
                    x.Order.TrackingNumber,
                    null,
                    null,
                    x.Order.CreatedOnUtc,
                    x.Order.LastModifiedOnUtc
                ));
            }

            // Pagination
            var pagedResult = await PagedResult<OrderSummaryDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

            return Result.Success(pagedResult);
        }

    }
}
