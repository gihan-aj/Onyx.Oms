namespace Onyx.Oms.Core.Domain.ValueObjects
{
    public record Weight
    {
        public decimal Value { get; init; }
        public string Unit { get; init; } = "kg";

        public Weight(decimal value, string unit = "kg")
        {
            if (value < 0) throw new ArgumentException("Weight cannot be negative.", nameof(value));
            if (string.IsNullOrWhiteSpace(unit)) throw new ArgumentException("Unit cannot be empty.", nameof(unit));
            Value = value;
            Unit = unit.ToLowerInvariant();
        }

        public override string ToString() => $"{Value} {Unit}";

        public static Weight Zero(string unit = "kg") => new Weight(0, unit);
    }
}
