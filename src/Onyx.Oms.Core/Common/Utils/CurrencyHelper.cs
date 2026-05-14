namespace Onyx.Oms.Core.Common.Utils
{
    public static class CurrencyHelper
    {
        public static string GetSymbol(string isoCurrencyCode)
        {
            if (string.IsNullOrWhiteSpace(isoCurrencyCode)) return "$";

            return isoCurrencyCode.ToUpperInvariant() switch
            {
                "LKR" => "Rs.",
                "USD" => "$",
                "EUR" => "€",
                "GBP" => "£",
                "AUD" => "A$",
                "INR" => "₹",
                "JPY" => "¥",
                _ => isoCurrencyCode // Fallback: just print the code (e.g., "AED")
            };
        }
    }
}
