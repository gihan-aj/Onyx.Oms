using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.DeactivatePaymentMethod
{
    public class DeactivatePaymentMethodHandler : ICommandHandler<DeactivatePaymentMethodCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeactivatePaymentMethodHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(DeactivatePaymentMethodCommand request, CancellationToken cancellationToken)
        {
            var paymentMethod = await _context.PaymentMethodConfigs
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (paymentMethod == null)
            {
                return Result.Failure(Error.NotFound("PaymentMethod.NotFound", "Payment method not found."));
            }

            if (!paymentMethod.IsActive)
                return Result.Success();

            paymentMethod.Deactivate();

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
