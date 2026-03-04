namespace Onyx.Oms.Core.Domain.ValueObjects;

/// <summary>
/// Represents a name/value pair that describes a variant attribute, such as a product option or characteristic.
/// </summary>
/// <remarks>Instances of this class are considered equal if both the Name and Value properties are equal. This is
/// useful for scenarios where variant attributes need to be compared or used as keys in collections.</remarks>
public class VariantAttribute : IEquatable<VariantAttribute>
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public bool Equals(VariantAttribute? other)
    {
        if(other is null)
            return false;

        return Name == other.Name && Value == other.Value;
    }
}
