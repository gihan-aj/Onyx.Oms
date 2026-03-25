using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities
{
    public class TenantSubscription : AuditableEntity<Guid>
    {
        public Guid TenantId { get; private set; }
        public Guid SubscriptionId { get; private set; } // External or internal subscription reference
        public SubscriptionPlan Plan { get; private set; } = null!;

        public SubscriptionStatus Status { get; private set; }

        public DateTimeOffset? TrialEndAtUtc { get; private set; }
        public DateTimeOffset? CurrentPeriodEndUtc { get; private set; }

        private TenantSubscription() { }

        private TenantSubscription(Guid tenantId, Guid subscriptionId, SubscriptionPlan plan, DateTimeOffset? trialEndAtUtc)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            SubscriptionId = subscriptionId;
            Plan = plan;
            
            if (trialEndAtUtc.HasValue && trialEndAtUtc > DateTimeOffset.UtcNow)
            {
                Status = SubscriptionStatus.Trialing;
                TrialEndAtUtc = trialEndAtUtc;
                CurrentPeriodEndUtc = trialEndAtUtc;
            }
            else
            {
                Status = SubscriptionStatus.Active;
            }
        }

        public static Result<TenantSubscription> Create(Guid tenantId, Guid subscriptionId, SubscriptionPlan plan, DateTimeOffset? trialEndAtUtc = null)
        {
            if (tenantId == Guid.Empty)
                return Result.Failure<TenantSubscription>(Error.Validation("TenantSubscription.TenantIdRequired", "TenantId is required."));

            if (plan == null)
                return Result.Failure<TenantSubscription>(Error.Validation("TenantSubscription.PlanRequired", "SubscriptionPlan is required."));

            return Result.Success(new TenantSubscription(tenantId, subscriptionId, plan, trialEndAtUtc));
        }

        public void Activate()
        {
            Status = SubscriptionStatus.Active;
        }

        public void Cancel()
        {
            Status = SubscriptionStatus.Canceled;
        }

        public void MarkPastDue()
        {
            Status = SubscriptionStatus.PastDue;
        }

        public void UpdateCurrentPeriod(DateTimeOffset endUtc)
        {
            CurrentPeriodEndUtc = endUtc;
            
            // If we are updating the current period and it's in the future and previous status was PastDue, we can activate it
            if (Status == SubscriptionStatus.PastDue && endUtc > DateTimeOffset.UtcNow)
            {
                Status = SubscriptionStatus.Active;
            }
        }
    }
}
