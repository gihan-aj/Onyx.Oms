using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.GetFulfillmentTasksPaged
{
    public class GetFullfillmentTasksPagedHandler : IQueryHandler<GetFullfillmentTasksPagedQuery, PagedResult<FulfillmentTaskDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetFullfillmentTasksPagedHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<FulfillmentTaskDto>>> Handle(GetFullfillmentTasksPagedQuery request, CancellationToken cancellationToken)
        {
            var baseQuery =
                from t in _context.FulfillmentTasks.AsNoTracking()

                    // INNER JOIN: Every task must have a Variant and a Product
                join pv in _context.ProductVariants on t.ProductVariantId equals pv.Id
                join p in _context.Products on pv.ProductId equals p.Id

                // LEFT JOIN: optional user
                join u in _context.AppUsers on t.AssignedUserId equals u.Id into userGrp
                from u in userGrp.DefaultIfEmpty()

                    // LEFT JOIN: Order Item is optional
                join oi in _context.OrderItems on t.LinkedOrderItemId equals oi.Id into oiGrp
                from oi in oiGrp.DefaultIfEmpty()

                    // LEFT JOIN: Order (Requires OrderItem to exist)
                join o in _context.Orders on oi.OrderId equals o.Id into orderGrp
                from o in orderGrp.DefaultIfEmpty()

                select new
                {
                    Task = t,
                    Variant = pv,
                    Product = p,
                    User = u,
                    OrderItem = oi,
                    Order = o
                };

            if (request.Type.HasValue)
                baseQuery = baseQuery.Where(x => x.Task.Type == request.Type);

            if (request.Priority.HasValue)
                baseQuery = baseQuery.Where(x => x.Task.Priority == request.Priority);

            if (request.ExpectedCompletionDate.HasValue)
            {
                var targetDate = request.ExpectedCompletionDate.Value.Date;
                baseQuery = baseQuery.Where(x =>
                    x.Task.ExpectedCompletionDate.HasValue &&
                    x.Task.ExpectedCompletionDate.Value.Date == targetDate);
            }

            if (!request.ShowAllStatus)
            {
                baseQuery = baseQuery.Where(x =>
                    x.Task.Status != FulfillmentTaskStatus.Cancelled &&
                    x.Task.Status != FulfillmentTaskStatus.Ready);
            }

            if (request.CreatedAfter.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Task.CreatedOnUtc >= request.CreatedAfter.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.OrderNumber))
            {
                baseQuery = baseQuery.Where(x => x.Order != null && x.Order.OrderNumber == request.OrderNumber);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                //baseQuery = baseQuery.Where( x => x.Product.Name.ToLower().Contains(search));
                baseQuery = baseQuery.Where(x =>
                    x.Product.Name.ToLower().Contains(search) ||
                    (x.Order != null && x.Order.OrderNumber.ToLower().Contains(search)) ||
                    (x.Task.PurchaseOrderNumber != null && x.Task.PurchaseOrderNumber.ToLower().Contains(search)));
            }

            bool isDesc = request.SortOrder?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            string sortCol = request.SortColumn?.ToLower() ?? string.Empty;

            baseQuery = sortCol switch
            {
                "createddate" => isDesc
                    ? baseQuery.OrderByDescending(x => x.Task.CreatedOnUtc)
                    : baseQuery.OrderBy(x => x.Task.CreatedOnUtc),

                "priority" => isDesc
                    ? baseQuery.OrderByDescending(x => x.Task.Priority)
                    : baseQuery.OrderBy(x => x.Task.Priority),

                // Default sorting: Earliest completion date first, but push Urgent tasks to the top
                _ => isDesc
                    ? baseQuery.OrderByDescending(x => x.Task.ExpectedCompletionDate)
                    : baseQuery.OrderByDescending(x => x.Task.Priority).ThenBy(x => x.Task.ExpectedCompletionDate),
            };

            var projection = baseQuery.Select(x => new FulfillmentTaskDto(
                x.Task.Id,
                x.Task.Type,
                x.Variant.Id,
                x.Product.Name,
                x.Variant.Sku,
                x.Product.HasVariants,
                x.Variant.Attributes.Select(a => new VariantAttributeDto(a.Name, a.Value)).ToList(),
                x.Task.RequestedQuantity,
                x.Task.LinkedOrderItemId,
                x.Order != null ? x.Order.OrderNumber : null,
                x.Task.Cost,
                x.Task.AssignedUserId,
                x.User != null ? x.User.FirstName : null,
                x.User != null ? x.User.LastName : null,
                x.Task.PurchaseOrderNumber,
                x.Task.Notes,
                x.Task.ExpectedCompletionDate,
                x.Task.Priority,
                x.Task.Status,
                x.Task.CreatedOnUtc,
                x.Task.StartedQuantity,
                x.Task.CompletedQuantity,
                x.Task.ScrappedQuantity));

            var pagedResult = await PagedResult<FulfillmentTaskDto>.CreateAsync(projection, request.Page, request.PageSize, cancellationToken);

            return pagedResult;
        }
    }
}
