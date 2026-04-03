namespace Onyx.Oms.Core.Common.Interfaces;
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAtUtc { get; }
    Guid? DeletedBy { get; }
    void Delete(Guid userId);
}
