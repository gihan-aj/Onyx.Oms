using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteProductionTask;

public record CompleteProductionTaskCommand(Guid ProductionTaskId, int QuantityToComplete) : ICommand;
