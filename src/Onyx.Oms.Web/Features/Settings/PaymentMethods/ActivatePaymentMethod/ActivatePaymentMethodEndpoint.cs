using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.ActivatePaymentMethod
{
    public class ActivatePaymentMethodEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/payment-methods")
                .WithApiVersionSet(app.NewApiVersionSet("PaymentMethods").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/activate", async (ISender sender, Guid id) =>
            {
                Result result = await sender.Send(new ActivatePaymentMethodCommand(id));

                return result.ToMinimalApiResult();
            })
            .WithTags("PaymentMethods")
            .WithName("ActivatePaymentMethod")
            .WithSummary("Activate a payment method")
            .WithDescription("Activates a payment method")
            .HasPermission(Permissions.PaymentMethods.Activate);
        }
    }
}
