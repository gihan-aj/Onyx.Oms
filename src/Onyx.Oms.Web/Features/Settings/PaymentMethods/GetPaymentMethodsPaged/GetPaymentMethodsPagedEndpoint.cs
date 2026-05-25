using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.GetPaymentMethodsPaged
{
    public class GetPaymentMethodsPagedEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/payment-methods")
                .WithApiVersionSet(app.NewApiVersionSet("PaymentMethods").Build())
                .HasApiVersion(1);

            group.MapGet("", async (ISender sender, [AsParameters] GetPaymentMethodsPagedQuery query) =>
            {
                Result<PagedResult<PaymentMethodConfigDto>> result = await sender.Send(query);

                return result.ToMinimalApiResult();
            })
            .WithTags("PaymentMethods")
            .WithName("GetPaymentMethodsPaged")
            .WithSummary("Get Payment Method Configurations")
            .WithDescription("Retrieves a paginated list of payment method configurations with optional searching, sorting, and filtering.")
            .HasPermission(Permissions.PaymentMethods.View);

        }
    }
}