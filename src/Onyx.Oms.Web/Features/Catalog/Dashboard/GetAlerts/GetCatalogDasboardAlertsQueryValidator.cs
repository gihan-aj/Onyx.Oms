using FluentValidation;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetAlerts
{
    public class GetCatalogDasboardAlertsQueryValidator : AbstractValidator<GetCatalogDashboardAlertsQuery>
    {
        public GetCatalogDasboardAlertsQueryValidator()
        {
            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Low stock threshold must be a non-negative integer.");
            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .WithMessage("Limit must be a positive integer.");
        }
    }
}
