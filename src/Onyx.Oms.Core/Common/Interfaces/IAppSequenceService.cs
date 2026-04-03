using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Common.Interfaces;

public interface IAppSequenceService
{
    Task<Result<string>> GetNextNumberAsync(string prefix, CancellationToken cancellationToken = default);
    Task<long?> GetCurrentValueAsync(string sequenceId, CancellationToken ct = default);
    Task<Result> UpdateCurrentValueAsync(string sequenceId, long newValue, CancellationToken ct = default);
}
