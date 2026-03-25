using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.DeactivateProduct
{
    public record DeactivateProductCommand(Guid ProductId) : ICommand;
}
