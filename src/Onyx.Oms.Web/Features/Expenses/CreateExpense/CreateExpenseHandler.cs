using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.CreateExpense
{
    public class CreateExpenseHandler : ICommandHandler<CreateExpenseCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CreateExpenseHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
        {
            Guid? tenantId = _currentUserService.ActiveTenantId;
            if (tenantId == null)
                return Result.Failure<Guid>(Error.Unauthorized("User.TenantIdMissing", "Tenant Id not found."));

            Money amount = new Money(request.Amount, request.Currency);

            var expenseCreateResult = Expense.Create(
                tenantId.Value, 
                request.Category, 
                amount, 
                request.DateIncurred, 
                request.Reference, 
                request.Notes);
            if (expenseCreateResult.IsFailure)
                return Result.Failure<Guid>(expenseCreateResult.Error);

            var expense = expenseCreateResult.Value;
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync(cancellationToken);

            return expense.Id;
        }
    }
}
