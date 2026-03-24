using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductImages
{
    public record UpdateProductImagesCommand(
        Guid ProductId,
        List<UpdateProductImageDto> Images
    ) : ICommand;

    public record UpdateProductImageDto(
        Guid Id,
        string Url,
        int DisplayOrder,
        bool IsMain,
        string? OptionName = null,
        string? OptionValue = null
    );
}
