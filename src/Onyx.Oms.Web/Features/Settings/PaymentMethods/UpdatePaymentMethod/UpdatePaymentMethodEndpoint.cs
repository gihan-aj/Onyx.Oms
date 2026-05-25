using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.UpdatePaymentMethod
{
    public class UpdatePaymentMethodEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/payment-methods")
                .WithApiVersionSet(app.NewApiVersionSet("PaymentMethods").Build())
                .HasApiVersion(1);

            group.MapPut("/{id:guid}", async (ISender sender, Guid id, [FromBody] UpdatePaymentMethodsRquest request) =>
            {
                var command = new UpdatePaymentMethodCommand(id, request.DisplayName, request.FeeRate);
                Result result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
                .WithTags("PaymentMethods")
                .WithName("UpdatePaymentMethod")
                .WithSummary("Update Payment Method")
                .WithDescription("Update a payment method's display name and fee rate")
                .HasPermission(Permissions.PaymentMethods.Edit);

        }
    }

    public record UpdatePaymentMethodsRquest(string DisplayName, decimal FeeRate);
}
