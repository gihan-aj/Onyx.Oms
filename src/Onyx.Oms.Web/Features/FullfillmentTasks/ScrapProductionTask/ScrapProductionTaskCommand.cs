using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProductionTask;

public record ScrapProductionTaskCommand(Guid ProductionTaskId, int QuantityToScrap) : ICommand;
