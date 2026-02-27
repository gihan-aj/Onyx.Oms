namespace Onyx.Oms.Core.Domain.ValueObjects
{
    public record Money
    {
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;

        public Money(decimal amount, string currency = "LKR")
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency cannot be empty.", nameof(currency));
            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        public override string ToString() => $"{Currency} {Amount:N2}";

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency) throw new InvalidOperationException("Cannot add amounts with different currencies.");
            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency) throw new InvalidOperationException("Cannot subtract amounts with different currencies.");
            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public static Money Zero(string currency = "LKR") => new Money(0, currency);
    }
}
