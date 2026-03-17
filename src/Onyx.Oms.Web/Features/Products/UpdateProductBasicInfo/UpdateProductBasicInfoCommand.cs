using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductBasicInfo
{
    public record UpdateProductBasicInfoCommand(
        Guid Id,
        string Name,
        string? Description,
        string? BaseSku,
        Guid CategoryId,
        List<string> Tags
    ) : ICommand;
}
