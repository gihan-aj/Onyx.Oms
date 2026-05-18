using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.AddOrderPayment
{
    public class AddOrderPaymentHandler : ICommandHandler<AddOrderPaymentCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public AddOrderPaymentHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(AddOrderPaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order == null)
                return Result.Failure<Guid>(Error.NotFound("Order.NotFound", "Order not found."));

            var amount = new Money(request.Amount, request.Currency);

            var paymentConfig = await _context.PaymentMethodConfigs
                .FirstOrDefaultAsync(p => p.Type == request.Method, cancellationToken);

            Money? fixedFee = null;
            if (request.Method == PaymentMethod.CashOnDelivery)
            {
                fixedFee = order.ShippingCost;
            }

            var paymentResult = order.AddPayment(request.Method, amount, paymentConfig?.FeeRate ?? 0, request.Reference, request.Note, request.PaymentDate, fixedFee);
            if(paymentResult.IsFailure)
                return Result.Failure<Guid>(paymentResult.Error);

            var payment = paymentResult.Value;
            _context.OrderPayments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }
    }
}
