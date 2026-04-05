using Onyx.Oms.Core.Common.Interfaces;

namespace Onyx.Oms.Infrastructure.Persistence.Entities;

public class AppSequence
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
