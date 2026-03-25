using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductVariants
{
    public record UpdateProductVariantsCommand(
        Guid ProductId,
        List<UpdateProductVariantDto> Variants
    ) : ICommand;

    public record UpdateProductVariantDto(
        Guid Id,
        string? Sku,
        MoneyDto? Cost,
        MoneyDto? Price,
        WeightDto? Weight,
        int StockOnHand,
        bool IsActive
    );

    public record MoneyDto(decimal Amount, string Currency = "LKR");
    public record WeightDto(decimal Value, string Unit = "kg");
}
