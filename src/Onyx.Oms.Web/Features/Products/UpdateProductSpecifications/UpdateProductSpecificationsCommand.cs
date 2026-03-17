using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductSpecifications
{
    public record UpdateProductSpecificationsCommand(
        Guid Id,
        Dictionary<string, string> Specifications
    ) : ICommand;
}
