using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.ActivateProduct
{
    public record ActivateProductCommand(Guid ProductId) : ICommand;
}
