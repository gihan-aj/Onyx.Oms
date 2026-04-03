using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Common.Interfaces;

public interface IAppSequenceService
{
    void InitialzeDefaultSequences(Guid tenantId);
    Task<Result<string>> GetNextNumberAsync(string prefix, CancellationToken cancellationToken = default);
    Task<long?> GetCurrentValueAsync(string prefix, CancellationToken ct = default);
    Task<Result> UpdateCurrentValueAsync(string prefix, long newValue, CancellationToken ct = default);
}
