using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.ToggleProductVariants
{
    public record ToggleProductVariantsCommand(
        Guid Id,
        bool HasVariants
    ) : ICommand;
}
