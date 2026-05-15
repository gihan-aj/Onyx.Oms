using FluentValidation;

namespace Onyx.Oms.Web.Features.Catalog.Dashboard.GetSummary
{
    public class GetCatalogDashboardSummaryValidator : AbstractValidator<GetCatalogDashboardSummaryQuery>
    {
        public GetCatalogDashboardSummaryValidator()
        {
            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Low stock threshold must be a non-negative integer.");
        }
    }
}
