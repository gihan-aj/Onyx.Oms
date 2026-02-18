namespace Onyx.Oms.Core.Common.Models;

public interface IAuditableEntity
{
    DateTimeOffset CreatedOnUtc { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? LastModifiedOnUtc { get; set; }
    string? LastModifiedBy { get; set; }
}
