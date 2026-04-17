using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.ScrapProcurementTask;

public record ScrapProcurementTaskCommand(Guid ProcurementTaskId, int QuantityToScrap) : ICommand;
