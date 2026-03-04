namespace Onyx.Oms.Core.Domain.ValueObjects;

/// <summary>
/// Represents a selectable option for a product, such as color, size, or material, including its display order and
/// allowed values.
/// </summary>
/// <remarks>Use this class to define product attributes that customers can choose from, enabling dynamic
/// generation of user interface elements like dropdown lists or swatches. Each instance specifies the option's name,
/// its position in the display sequence, and the set of permissible values.</remarks>
public class ProductOption
{
    public string Name { get; set; } = string.Empty; // e.g., "Color", "Size", "Material"
    public int DisplayOrder {  get; set; }

    // The allowed values for this product. e.g. ["Red", "Blue", "Green"]
    // This helps UI generate dropdowns/swatches.
    public List<string> Values { get; set; } = new();
}
