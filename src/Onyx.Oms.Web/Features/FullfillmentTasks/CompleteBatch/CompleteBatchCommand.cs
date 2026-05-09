using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteBatch
{
    public record CompleteBatchCommand(Guid ProductVariantId, bool AllocateToOrders) : ICommand;
}
