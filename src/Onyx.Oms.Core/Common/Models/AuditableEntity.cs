namespace Onyx.Oms.Core.Common.Models;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
{
    protected AuditableEntity(TId id) : base(id)
    {
    }

    protected AuditableEntity()
    {
    }

    public DateTimeOffset CreatedOnUtc { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedOnUtc { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
