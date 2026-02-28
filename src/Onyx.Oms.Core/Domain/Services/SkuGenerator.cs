namespace Onyx.Oms.Core.Domain.Services;

public static class SkuGenerator
{
    public static string GenerateVariantSku(string baseSku, string? color, string? size)
    {
        // Guard Clauses for Optional Attributes
        string colorCode = string.IsNullOrWhiteSpace(color) ? "" : GetColorCode(color);
        string sizeCode = string.IsNullOrWhiteSpace(size) ? "" : GetSizeCode(size);

        var parts = new List<string> { baseSku };
        if (!string.IsNullOrEmpty(colorCode)) parts.Add(colorCode);
        if (!string.IsNullOrEmpty(sizeCode)) parts.Add(sizeCode);

        return string.Join("-", parts).ToUpperInvariant();
    }

    private static string GetSizeCode(string size)
    {
        return size.Trim().ToUpperInvariant() switch
        {
            "SMALL" => "S",
            "MEDIUM" => "M",
            "LARGE" => "L",
            "EXTRA LARGE" => "XL",
            _ => size.Length <= 3 ? size.ToUpperInvariant() : size.Substring(0, 3).ToUpperInvariant()
        };
    }

    private static string GetColorCode(string color)
    {
        // 1. Standard Dictionary
        var knownColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Black", "BLK" }, { "White", "WHT" }, { "Red", "RED" }, 
            { "Navy", "NVY" }, { "Blue", "BLU" }, { "Green", "GRN" },
            { "Grey", "GRY" }, { "Gray", "GRY" }, { "Purple", "PRP" }
        };
        
        if (knownColors.TryGetValue(color, out var code)) return code;

        // 2. Consonant Extraction Algorithm
        // Remove vowels and spaces
        var consonants = new string(color.Where(c => !"AEIOUaeiou ".Contains(c)).ToArray());
        
        // Always keep the first letter of the original word
        string firstLetter = color.Substring(0, 1);
        string remainingConsonants = consonants.Length > 1 ? consonants.Substring(1) : "";
        
        string result = firstLetter + remainingConsonants;
        
        // Pad or Truncate to 3 chars
        if (result.Length >= 3) return result.Substring(0, 3).ToUpperInvariant();
        return result.PadRight(3, 'X').ToUpperInvariant(); 
    }
}
