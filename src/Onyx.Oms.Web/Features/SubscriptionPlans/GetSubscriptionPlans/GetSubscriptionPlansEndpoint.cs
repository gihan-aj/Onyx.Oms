using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.SubscriptionPlans.GetSubscriptionPlans
{
    public class GetSubscriptionPlansEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/subscription-plans")
                .WithApiVersionSet(app.NewApiVersionSet("SubscriptionPlans").Build())
                .HasApiVersion(1);

            group.MapGet("", async (ISender sender, [FromBody] GetSubscriptionPlansQuery command) =>
            {
                var result = await sender.Send(command);
                return result.ToMinimalApiResult();
            })
            .WithTags("SubscriptionPlans")
            .WithName("GetSubscriptionPlans")
            .WithSummary("Get all Subscription Plans");
        }
    }
}
