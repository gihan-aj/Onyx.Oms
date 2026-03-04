using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;

namespace Onyx.Oms.Core.Domain.Entities;

public class ProductImage : Entity<Guid>
{
    public ProductImage(Guid productId, string url, int displayOrder, bool isMain) : base(Guid.NewGuid())
    {
        ProductId = productId;
        Url = url;
        DisplayOrder = displayOrder;
        IsMain = isMain;
    }

    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsMain { get; private set; }
    public string? OptionName { get; private set; }
    public string? OptionValue { get; private set; }

    public virtual Product Product { get; private set; } = null!;

    public Result LinkToOption(string optionName, string value, IReadOnlyCollection<ProductOption> validOptions)
    {
        var option = validOptions.FirstOrDefault(o => o.Name == optionName);
        if (option is null)
            return Result.Failure(Error.Validation("Image.InvalidOption", $"Option '{optionName}' does not exist on this product."));

        if (!option.Values.Contains(value))
            return Result.Failure(Error.Validation("Image.InvalidValue", $"Value '{value}' is not valid for option '{optionName}'."));

        OptionName = optionName;
        OptionValue = value;
        return Result.Success();
    }

    public void Unlink()
    {
        OptionName = null;
        OptionValue = null;
    }

    // Helper to check if this image applies to a specific variant
    public bool AppliesToVariant(ProductVariant variant)
    {
        // If this image is generic, it applies to everyone
        if (string.IsNullOrEmpty(OptionName)) return true;

        // Otherwise, the variant must have an attribute that matches this image's tag
        // e.g. Image is for "Color: Red", does Variant have Attribute "Color" == "Red"?
        return variant.Attributes.Any(a =>
            a.Name == OptionName &&
            a.Value == OptionValue);
    }
}
