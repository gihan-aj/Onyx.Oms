using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Reports.GetMonthlyFinancialReport
{
    public class GetMonthlyFinancialReportEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/reports")
                .WithApiVersionSet(app.NewApiVersionSet("Reports").Build())
                .HasApiVersion(1);

            group.MapGet("monthly-financials", async (ISender sender, [FromQuery] int year, [FromQuery] int month) =>
            {
                var query = new GetMonthlyFinancialReportQuery(year, month);
                Result<MonthlyFinancialReportDto> result = await sender.Send(query);
                return result.ToMinimalApiResult();
            })
            .WithTags("Reports")
            .WithName("GetMonthlyFinancials")
            .WithSummary("Get monthly Profit & Loss report")
            .HasPermission(Permissions.Reports.MonthlyFinancialsView);
        }
    }
}
