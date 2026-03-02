using Onyx.Oms.Core.Domain.Enums;

namespace Onyx.Oms.Core.Domain.ValueObjects;

public class SpecDefinition
{
    public string Key { get; set; } = string.Empty;     // "gender"
    public string Label { get; set; } = string.Empty;   // "Target Gender"
    public SpecType Type { get; set; } = SpecType.Text;
    public bool IsRequired { get; set; } = false;

    // For Select / MultiSelect options
    public List<string> Options { get; set; } = new();
}
