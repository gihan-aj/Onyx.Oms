using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.CancelProcurementTask;

public record CancelProcurementTaskCommand(Guid ProcurementTaskId) : ICommand;
