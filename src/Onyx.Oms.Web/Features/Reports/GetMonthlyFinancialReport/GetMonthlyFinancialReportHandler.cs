using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Reports.GetMonthlyFinancialReport
{
    public class GetMonthlyFinancialReportHandler : IQueryHandler<GetMonthlyFinancialReportQuery, MonthlyFinancialReportDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMonthlyFinancialReportHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<MonthlyFinancialReportDto>> Handle(GetMonthlyFinancialReportQuery request, CancellationToken cancellationToken)
        {
            var profile = await _context.Tenants
                .Where(t => t.Id == _currentUserService.ActiveTenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (profile == null)
            {
                return Result.Failure<MonthlyFinancialReportDto>(Error.NotFound("Tenant.NotFound", "Tenant details not found."));
            }

            var currency = profile.DefaultCurrency;

            var startDate = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endDate = startDate.AddMonths(1);

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .Where(o => o.Status == OrderStatus.Completed || o.PaymentStatus == PaymentStatus.FullyPaid)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            int totalOrders = orders.Count();
            decimal grossSales = orders.SelectMany(o => o.Items).Sum(i => i.UnitPrice.Amount * i.Quantity);
            decimal totalDiscounts = orders.Sum(o => o.DiscountAmount.Amount) + orders.SelectMany(o => o.Items).Sum(i => i.DiscountAmount.Amount);
            decimal shippingRevenue = orders.Sum(o => o.ShippingCost.Amount);

            decimal netRevenue = (grossSales + shippingRevenue) - totalDiscounts;

            decimal cogs = orders.SelectMany(o => o.Items).Sum(i => (i.UnitCost?.Amount ?? 0) * i.Quantity);

            decimal grossProfit = netRevenue - cogs;
            decimal grossMargin = netRevenue > 0 ? (grossProfit / netRevenue) * 100m : 0m;

            var expenses = await _context.Expenses
                .Where(e => e.DateIncurred >= startDate && e.DateIncurred < endDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            decimal totalExpenses = expenses.Sum(e => e.Amount.Amount);

            var expensesByCategory = expenses
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseCategorySummaryDto(g.Key, g.Sum(e => e.Amount.Amount)))
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            decimal netProfit = grossProfit - totalExpenses;
            decimal netMargin = netRevenue > 0 ? (netProfit / netRevenue) * 100m : 0m;

            var report = new MonthlyFinancialReportDto(
                currency,
                request.Year,
                request.Month,
                totalOrders,
                grossSales,
                totalDiscounts,
                shippingRevenue,
                netRevenue,
                cogs,
                grossProfit,
                Math.Round(grossMargin, 2),
                totalExpenses,
                expensesByCategory,
                netProfit,
                Math.Round(netMargin, 2)
            );

            return Result.Success(report);
        }
    }
}
