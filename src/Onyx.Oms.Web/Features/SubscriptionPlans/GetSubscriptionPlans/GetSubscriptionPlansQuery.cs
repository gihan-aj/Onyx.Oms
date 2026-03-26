using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.SubscriptionPlans.GetSubscriptionPlans
{
    public record GetSubscriptionPlansQuery() : IQuery<List<SubscriptionPlanResponse>>
    {
        public bool? IsActive { get; init; }
    }

    public record SubscriptionPlanResponse(
        Guid Id,
        string Name,
        MoneyDto MonthlyPrice,
        int MaxUsersAllowed,
        int MaxOrdersAllowed,
        int TrialPeriodInDays,
        bool IsActive);

    public record MoneyDto(decimal Amount, string Currency);
}
