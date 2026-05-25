using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.ActivatePaymentMethod
{
    public class ActivatePaymentMethodHandler : ICommandHandler<ActivatePaymentMethodCommand>
    {
        private readonly IApplicationDbContext _context;

        public ActivatePaymentMethodHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(ActivatePaymentMethodCommand request, CancellationToken cancellationToken)
        {
            var paymentMethod = await _context.PaymentMethodConfigs
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (paymentMethod == null)
            {
                return Result.Failure(Error.NotFound("PaymentMethod.NotFound", "Payment method not found."));
            }

            if (paymentMethod.IsActive)
                return Result.Success();

            paymentMethod.Activate();

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
