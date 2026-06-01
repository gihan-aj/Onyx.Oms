using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities
{
    public class Expense : AuditableEntity<Guid>, IMustHaveTenant, ISoftDeletable
    {
        public Guid TenantId { get; private set; }

        public string Category { get; private set; } = string.Empty;
        public Money Amount { get; private set; } = Money.Zero();
        public DateTimeOffset DateIncurred { get; private set; }

        public string? Reference {  get; private set; } // Invoice #, Receipt ID
        public string? Notes { get; private set; }

        public bool IsDeleted => DeletedAtUtc is not null;
        public DateTimeOffset? DeletedAtUtc { get; private set; }
        public Guid? DeletedBy { get; private set; }

        private Expense() { }

        private Expense(
            Guid tenantId,
            string category,
            Money amount,
            DateTimeOffset dateIncurred,
            string? reference,
            string? notes) : base(Guid.NewGuid())
        {
            TenantId = tenantId;
            Category = category.Trim();
            Amount = amount;
            DateIncurred = dateIncurred;
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        public static Result<Expense> Create(
            Guid tenantId,
            string category,
            Money amount,
            DateTimeOffset dateIncurred,
            string? reference,
            string? notes)
        {
            if (string.IsNullOrWhiteSpace(category))
                return Result.Failure<Expense>(Error.Validation("Expense.CategoryRequired", "Expense category is required."));

            if (amount.Amount <= 0)
                return Result.Failure<Expense>(Error.Validation("Expense.InvalidAmount", "Expense amount must be greater than zero."));

            return Result.Success(new Expense(tenantId, category, amount, dateIncurred, reference, notes));
        }

        public Result Update(
            string category,
            Money amount,
            DateTimeOffset dateIncurred,
            string? reference,
            string? notes)
        {
            if (IsDeleted)
                return Result.Failure(Error.Validation("Expense.IsDeleted", "Cannot update a deleted expense."));

            if (string.IsNullOrWhiteSpace(category))
                return Result.Failure(Error.Validation("Expense.CategoryRequired", "Expense category is required."));

            if (amount.Amount <= 0)
                return Result.Failure(Error.Validation("Expense.InvalidAmount", "Expense amount must be greater than zero."));

            Category = category.Trim();
            Amount = amount;
            DateIncurred = dateIncurred;
            Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

            return Result.Success();
        }

        public void Delete(Guid userId)
        {
            if(IsDeleted) return;

            DeletedAtUtc = DateTimeOffset.UtcNow;
            DeletedBy = userId;
        }
    }
}
