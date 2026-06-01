using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.UpdateExpense
{
    public class UpdateExpenseHandler : ICommandHandler<UpdateExpenseCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateExpenseHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (expense == null)
                return Result.Failure(Error.NotFound("Expense.NotFound", "Expense record is not found."));

            var amount = new Money(request.Amount, request.Currency);

            var updateResult = expense.Update(
                request.Category,
                amount,
                request.DateIncurred,
                request.Reference,
                request.Notes);
            if (updateResult.IsFailure)
                return updateResult;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}