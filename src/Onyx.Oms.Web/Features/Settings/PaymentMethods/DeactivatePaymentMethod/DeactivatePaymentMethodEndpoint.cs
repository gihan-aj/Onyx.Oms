using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.DeactivatePaymentMethod
{
    public class DeactivatePaymentMethodEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/payment-methods")
                .WithApiVersionSet(app.NewApiVersionSet("PaymentMethods").Build())
                .HasApiVersion(1);

            group.MapPut("{id:guid}/deactivate", async (ISender sender, Guid id) =>
            {
                Result result = await sender.Send(new DeactivatePaymentMethodCommand(id));

                return result.ToMinimalApiResult();
            })
            .WithTags("PaymentMethods")
            .WithName("DeactivatePaymentMethod")
            .WithSummary("Deactivate a payment method")
            .WithDescription("Deactivates a payment method")
            .HasPermission(Permissions.PaymentMethods.Deactivate);
        }
    }
}
