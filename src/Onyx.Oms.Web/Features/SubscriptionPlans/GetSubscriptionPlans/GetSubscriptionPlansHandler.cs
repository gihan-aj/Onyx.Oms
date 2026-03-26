using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.SubscriptionPlans.GetSubscriptionPlans
{
    public class GetSubscriptionPlansHandler : IQueryHandler<GetSubscriptionPlansQuery, List<SubscriptionPlanResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetSubscriptionPlansHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SubscriptionPlanResponse>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
        {
            var plansQuery = _context.SubscriptionPlans
                .AsNoTracking()
                .AsQueryable();

            if (request.IsActive.HasValue)
            {
                plansQuery = plansQuery
                    .Where(s => s.IsActive == request.IsActive);
            }

            var plans = await plansQuery
                .Select(s => new SubscriptionPlanResponse(
                    s.Id,
                    s.Name,
                    new MoneyDto(s.MonthlyPrice.Amount, s.MonthlyPrice.Currency),
                    s.MaxUsersAllowed,
                    s.MaxOrdersAllowed,
                    s.TrialPeriodInDays,
                    s.IsActive))
                .ToListAsync(cancellationToken);

            return plans;
        }
    }
}
