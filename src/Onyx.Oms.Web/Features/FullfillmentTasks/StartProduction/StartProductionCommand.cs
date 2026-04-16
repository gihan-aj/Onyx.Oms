using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.StartProduction
{
    public record StartProductionCommand(
        Guid ProductionsTaskId,
        int QuantityToStart) : ICommand;
}
