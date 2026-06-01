using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.DeleteExpense
{
    public class DeleteExpenseHandler : ICommandHandler<DeleteExpenseCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public DeleteExpenseHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return Result.Failure(Error.Unauthorized("User.IdNotFound", "User ID is not found."));

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == request.ExpenseId, cancellationToken);
            if (expense == null)
                return Result.Failure(Error.NotFound("Expense.NotFound", "Expense record is not found."));

            expense.Delete(userId.Value);

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
