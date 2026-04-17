using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CancelProductionTask;

public record CancelProductionTaskCommand(Guid ProductionTaskId) : ICommand;
