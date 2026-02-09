using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Domain.Entities;

public class ProductImage : Entity
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

    public virtual Product Product { get; private set; } = null!;
}
