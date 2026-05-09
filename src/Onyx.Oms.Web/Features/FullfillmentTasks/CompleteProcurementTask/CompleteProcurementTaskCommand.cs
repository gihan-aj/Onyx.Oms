using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CompleteProcurementTask;

public record CompleteProcurementTaskCommand(Guid ProcurementTaskId, int QuantityToComplete, bool? AllocateToOrder = null) : ICommand;
