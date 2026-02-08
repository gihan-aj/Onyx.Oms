namespace Onyx.Oms.Core.Common.Models;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id) : base(id)
    {
    }

    protected AuditableEntity()
    {
    }

    public DateTimeOffset CreatedOnUtc { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModifiedOnUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
