using FluentValidation;
using MediatR;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Constants;
using Onyx.Oms.Web.Common;
using Onyx.Oms.Web.Extensions;

namespace Onyx.Oms.Web.Features.Orders.AddOrderPayment
{
    public class AddOrderPaymentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v{version:apiVersion}/orders")
               .WithApiVersionSet(app.NewApiVersionSet("Orders").Build())
               .HasApiVersion(1);

            group.MapPost("/{id:guid}/payments", async (Guid id, AddOrderPaymentRequest request, ISender sender) =>
            {
                var command = new AddOrderPaymentCommand(
                    id,
                    request.Amount,
                    request.Currency,
                    request.Method,
                    request.Reference,
                    request.Note,
                    request.PaymentDate);

                Result<Guid> result = await sender.Send(command);

                return result.ToMinimalApiResult();
            })
                .WithTags("Orders")
                .WithName("AddOrderpayment")
                .WithSummary("Add payment to an order")
                .WithDescription("Add a payment transaction through order details screen.")
                .Produces<Guid>()
                .HasPermission(Permissions.Orders.Edit);
        }
    }
}
