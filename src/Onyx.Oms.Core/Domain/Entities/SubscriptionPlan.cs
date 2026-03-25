using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities
{
    public class SubscriptionPlan : AuditableEntity<Guid>
    {
        public  string Name { get; private set; } = string.Empty;
        public Money MonthlyPrice { get; private set; } = Money.Zero();

        public int MaxUsersAllowed { get; private set; }
        public int MaxOrdersAllowed { get; private set; }

        public int TrialPeriodInDays { get; private set; }

        public bool IsActive { get; private set; }

        private SubscriptionPlan() { }

        private SubscriptionPlan(string name, Money monthlyPrice, int maxUsersAllowed, int maxOrdersAllowed, int trialPeriodInDays)
        {
            Id = Guid.NewGuid();
            Name = name;
            MonthlyPrice = monthlyPrice;
            MaxUsersAllowed = maxUsersAllowed;
            MaxOrdersAllowed = maxOrdersAllowed;
            TrialPeriodInDays = trialPeriodInDays;
            IsActive = true;
        }

        public static Result<SubscriptionPlan> Create(string name, Money monthlyPrice, int maxUsersAllowed, int maxOrdersAllowed, int trialPeriodInDays)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure<SubscriptionPlan>(Error.Validation("SubscriptionPlan.NameRequired", "Plan Name is required."));

            return Result.Success(new SubscriptionPlan(name, monthlyPrice, maxUsersAllowed, maxOrdersAllowed, trialPeriodInDays));
        }
    }
}
