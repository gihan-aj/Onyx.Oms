using System.Text;

namespace Onyx.Oms.Core.Domain.Services;

public static class SkuGenerator
{
    private static readonly Dictionary<string, string> _knownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        // --- Colors ---
        { "Black", "BLK" }, { "White", "WHT" }, { "Red", "RED" },
        { "Blue", "BLU" }, { "Green", "GRN" }, { "Yellow", "YLW" },
        { "Orange", "ORG" }, { "Purple", "PRP" }, { "Pink", "PNK" },
        { "Brown", "BRN" }, { "Beige", "BGE" }, { "Grey", "GRY" },
        { "Gray", "GRY" }, { "Gold", "GLD" }, { "Silver", "SLV" },
        { "Navy", "NVY" }, { "Teal", "TEL" }, { "Maroon", "MRN" },
        { "Olive", "OLV" }, { "Cyan", "CYN" }, { "Magenta", "MAG" },
        { "Cream", "CRM" }, { "Charcoal", "CHR" }, { "Coral", "CRL" },
        { "Khaki", "KHK" }, { "Lavender", "LAV" }, { "Mint", "MNT" },
        { "Peach", "PCH" }, { "Turquoise", "TRQ" }, { "Violet", "VLT" },
        { "Burgundy", "BUR" }, { "Ivory", "IVR" }, { "Multi", "MUL" },

        // --- Standard Sizes ---
        { "Extra Small", "XSM" }, { "XS", "XSM" },
        { "Small", "SML" },       { "S", "SML" },
        { "Medium", "MED" },      { "M", "MED" },
        { "Large", "LRG" },       { "L", "LRG" },
        { "Extra Large", "XLG" }, { "XL", "XLG" },
        { "XXL", "2XL" },         { "2XL", "2XL" },
        { "XXXL", "3XL" },        { "3XL", "3XL" },
        { "One Size", "OSZ" },    { "Free Size", "FSZ" },

        // --- Materials / Common ---
        { "Cotton", "CTN" }, { "Polyester", "PLY" }, { "Leather", "LTR" },
        { "Wool", "WOL" }, { "Silk", "SLK" }, { "Denim", "DNM" },
        { "Linen", "LIN" }, { "Velvet", "VLV" }, { "Nylon", "NYL" },
        { "Metal", "MTL" }, { "Wood", "WOD" }, { "Plastic", "PLC" },
        { "Glass", "GLS" }, { "Ceramic", "CER" }
    };

    /// <summary>
    /// Generates a 3-character code for a given option value.
    /// E.g., "Dark Blue" -> "DKB" (via algo) or "Black" -> "BLK" (via dict).
    /// </summary>
    public static string GetOptionValueCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "XXX";

        value = value.Trim();

        // 1. Try Lookup
        if (_knownValues.TryGetValue(value, out var code))
            return code;

        // 2. Fallback: Consonant Extraction Algorithm
        return GenerateConsonantCode(value);
    }

    private static string GenerateConsonantCode(string value)
    {
        string upper = value.ToUpperInvariant();

        // Strategy: 
        // 1. Keep first char.
        // 2. Remove vowels (A, E, I, O, U) from the REST.
        // 3. Remove spaces and special chars.

        if (upper.Length == 0) return "XXX";

        var sb = new StringBuilder();

        // Always take the first character
        char firstChar = upper[0];
        sb.Append(firstChar);

        // Process remainder
        for (int i = 1; i < upper.Length; i++)
        {
            char c = upper[i];

            // Skip non-letters (spaces, numbers) - OR you can decide to keep numbers
            if (!char.IsLetterOrDigit(c)) continue;

            // Skip vowels
            if ("AEIOU".Contains(c)) continue;

            sb.Append(c);

            // Optimization: Stop once we have enough for a 3-char code + buffer
            if (sb.Length >= 4) break;
        }

        string result = sb.ToString();

        // 3. Final Formatting (Pad or Truncate)
        if (result.Length >= 3)
            return result.Substring(0, 3);

        // If we stripped too much and have < 3 (e.g. "Ox" -> "X"), pad with X
        return result.PadRight(3, 'X');
    }
}
