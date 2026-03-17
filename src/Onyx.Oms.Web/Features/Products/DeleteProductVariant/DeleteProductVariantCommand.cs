using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.DeleteProductVariant
{
    public record DeleteProductVariantCommand(
        Guid ProductId,
        Guid VariantId
    ) : ICommand;
}
