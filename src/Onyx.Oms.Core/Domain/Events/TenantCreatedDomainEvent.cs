using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Core.Domain.Events
{
    public record TenantCreatedDomainEvent(Guid TenantId) : IDomainEvent;
}
