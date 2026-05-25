using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.GetPaymentMethodsPaged
{
    public class GetPaymentMethodsPagedHandler : IQueryHandler<GetPaymentMethodsPagedQuery, PagedResult<PaymentMethodConfigDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPaymentMethodsPagedHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResult<PaymentMethodConfigDto>>> Handle(GetPaymentMethodsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _context.PaymentMethodConfigs
                .AsNoTracking();

            if (request.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(p => p.DisplayName.Contains(request.SearchTerm));
            }

            bool isDesc = request.SortOrder?.ToLower() == "desc";

            if (string.IsNullOrWhiteSpace(request.SortColumn))
            {
                query = isDesc ? query.OrderByDescending(p => p.DisplayName) : query.OrderBy(p => p.DisplayName);

            }
            else
            {
                query = request.SortColumn.ToLower() switch
                {
                    "displayname" => isDesc ? query.OrderByDescending(p => p.DisplayName) : query.OrderBy(p => p.DisplayName),
                    "feerate" => isDesc ? query.OrderByDescending(p => p.FeeRate) : query.OrderBy(p => p.FeeRate),
                    "isactive" => isDesc ? query.OrderByDescending(p => p.IsActive) : query.OrderBy(p => p.IsActive),
                    _ => query.OrderBy(p => p.DisplayName)
                };
            }

            var dtoQuery = query.Select(p => new PaymentMethodConfigDto(
                p.Id,
                p.Type,
                p.DisplayName,
                p.FeeRate,
                p.IsActive
            ));

            var pagedResult = await PagedResult<PaymentMethodConfigDto>.CreateAsync(dtoQuery, request.Page, request.PageSize, cancellationToken);

            return Result.Success(pagedResult);
        }
    }
}