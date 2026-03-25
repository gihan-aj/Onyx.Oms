using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding;

public class SubscriptionPlanSeeder
{
    private readonly IApplicationDbContext _context;

    public SubscriptionPlanSeeder(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var freePlanName = "Free Tier";

        var freePlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == freePlanName);

        if (freePlan == null)
        {
            var planResult = SubscriptionPlan.Create(
                name: freePlanName,
                monthlyPrice: Money.Zero(),
                maxUsersAllowed: 5,
                maxOrdersAllowed: 100,
                trialPeriodInDays: 0 // Will not expire
            );

            if (planResult.IsSuccess)
            {
                _context.SubscriptionPlans.Add(planResult.Value);
                await _context.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
