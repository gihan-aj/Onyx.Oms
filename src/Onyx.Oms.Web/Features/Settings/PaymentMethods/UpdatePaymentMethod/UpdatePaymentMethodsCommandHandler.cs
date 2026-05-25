using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Settings.PaymentMethods.UpdatePaymentMethod
{
    public class UpdatePaymentMethodsCommandHandler : ICommandHandler<UpdatePaymentMethodCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdatePaymentMethodsCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Result> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
        {
            var paymentMethod = await _context.PaymentMethodConfigs
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (paymentMethod == null)
            {
                return Result.Failure(Error.NotFound("PaymentMethod.NotFound", "Payment method not found."));
            }

            var updateResult = paymentMethod.Update(request.DisplayName, request.FeeRate);
            if(updateResult.IsFailure)
                return updateResult;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
