namespace Onyx.Oms.Infrastructure.Persistence.Entities;

public class AppSequence
{
    public string Id { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
}
