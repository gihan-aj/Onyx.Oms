namespace Onyx.Oms.Core.Common.Models;

public interface IAuditableEntity
{
    DateTimeOffset CreatedOnUtc { get; set; }
    Guid CreatedBy { get; set; }
    DateTimeOffset? LastModifiedOnUtc { get; set; }
    Guid? LastModifiedBy { get; set; }
}
