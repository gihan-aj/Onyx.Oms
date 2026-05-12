using Onyx.Oms.Core.Common.Models;

namespace Onyx.Oms.Core.Common.Interfaces
{
    public interface IProductSheetGenerator
    {
        Task<Result<byte[]>> GenerateAsync(Guid productId, string imageStoragePath, CancellationToken cancellationToken = default);
    }
}
