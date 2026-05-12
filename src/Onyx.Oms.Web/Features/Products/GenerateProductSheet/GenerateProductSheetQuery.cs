using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.GenerateProductSheet
{
    public record GenerateProductSheetQuery(Guid ProductId, string ImageStoragePath) : IQuery<byte[]>;
}
