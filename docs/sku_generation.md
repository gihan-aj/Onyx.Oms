# SKU & Variant Code Generation Logic - Onyx.Oms

## 1. SKU Architecture

The Stock Keeping Unit (SKU) is the primary identifier for inventory tracking. In the Onyx system, Variant SKUs are composed using a standard concatenation pattern to ensuring readability and consistency.

**Format:** `[BaseSku]-[ColorCode]-[SizeCode]`

**Examples:**
* Base: `TSHIRT-001`, Color: `Red`, Size: `Medium` $\rightarrow$ **`TSHIRT-001-RED-M`**
* Base: `PANT-X99`, Color: `Navy Blue`, Size: `32` $\rightarrow$ **`PANT-X99-NVY-32`**

## 2. Generation Algorithms

The system uses a "Smart Suggestion" strategy. It attempts to generate a human-readable code first, but always allows the user to intervene.

### 2.1 Color Code Logic
The system determines the 3-letter Color Code using a prioritized strategy:

1.  **Exact Dictionary Match:** Checks a predefined list of standard industry colors.
    * `Black` $\rightarrow$ `BLK`
    * `White` $\rightarrow$ `WHT`
    * `Red` $\rightarrow$ `RED`
    * `Navy` / `Navy Blue` $\rightarrow$ `NVY`
    * `Green` $\rightarrow$ `GRN`
    * `Yellow` $\rightarrow$ `YLW`
2.  **Consonant Extraction (The Fallback):** If the color is custom (e.g., "Charcoal"), the system generates a code by taking the first letter and the next two consonants, skipping vowels.
    * `Charcoal` $\rightarrow$ `C` + `h` `r` $\rightarrow$ **`CHR`**
    * `Crimson` $\rightarrow$ `C` + `r` `m` $\rightarrow$ **`CRM`**
    * `Peach` $\rightarrow$ `P` + `c` `h` $\rightarrow$ **`PCH`**
3.  **Truncation:** If the word has no consonants or is short, it falls back to the first 3 letters or pads with 'X'.
    * `Blue` $\rightarrow$ **`BLU`**

### 2.2 Size Code Logic
Sizes are standardized to prevent variations like "Med" vs "M".

1.  **Standard Apparel Mapping:**
    * `Small` $\rightarrow$ `S`
    * `Medium` $\rightarrow$ `M`
    * `Large` $\rightarrow$ `L`
    * `Extra Large` / `XL` $\rightarrow$ `XL`
    * `XXL` / `2XL` $\rightarrow$ `XXL`
2.  **Numeric Passthrough:** If the size is numeric (e.g., "32", "40", "10.5"), it is used as-is.

## 3. Business Rules & UI Workflow

### 3.1 The "Suggest & Dispose" Rule
* **Trigger:** When a user selects a Color and Size in the "Add Variant" dialog (or the Variant Matrix).
* **Action:** The ViewModel runs the `SkuGenerator` and auto-populates the `SKU` TextBox.
* **User Override:** The TextBox remains **editable**. If the item already has a manufacturer barcode (e.g., Nike uses their own SKUs), the user can wipe the suggested SKU and scan the real barcode into the field.

### 3.2 Uniqueness Constraint
* **Scope:** Tenant-wide.
* **Validation:** The system prevents saving if the generated SKU already exists for *any* other product.

### 3.3 Base SKU Mutability (The Cascade Warning)
* **Scenario:** User changes the Base Product SKU from `T01` to `T02`.
* **Logic:**
    * By default, **ONLY** the `Product.BaseSku` is updated.
    * Existing variants (`T01-RED-M`) are **NOT** automatically changed to preserve compatibility with existing printed labels.
    * **UI Option:** The user is presented with a checkbox: *"Update existing variant SKUs?"*. If checked, a warning is shown: *"Warning: This will invalidate printed barcodes."*

## 4. C# Implementation (Domain Service)

```csharp
public static class SkuGenerator
{
    public static string GenerateVariantSku(string baseSku, string color, string size)
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
```