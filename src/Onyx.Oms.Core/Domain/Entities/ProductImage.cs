using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class ProductImage : Entity<Guid>
{
    public ProductImage(Guid id, Guid productId, string url, int displayOrder, bool isMain) : base(id)
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
    public string? Color { get; private set; } 

    public virtual Product Product { get; private set; } = null!;

    public Result TagWithColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return Result.Failure(Error.Validation("ProductImage.EmptyColorName", "Color is required."));

        Color = color;
        return Result.Success();
    }

    public void RemoveColorTag()
    {
        Color = null;
    }
}
