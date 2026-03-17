using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductOptions
{
    public record UpdateProductOptionsCommand(
        Guid Id,
        List<UpdateProductOptionDto> Options
    ) : ICommand;

    public record UpdateProductOptionDto(string Name, List<string> Values);
}
