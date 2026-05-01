using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.UpdateOrderFinancials
{
    public record UpdateOrderFinancialsCommand(
        Guid OrderId,
        List<UpdateOrderItemDto> Items,
        UpdateMoneyDto? ShippingFee,
        UpdateMoneyDto? TaxAmount,
        UpdateOrderDiscountDto? Discount) : ICommand;

    public record UpdateOrderItemDto(
        Guid? Id,
        Guid ProductVariantId,
        int Quantity,
        UpdateOrderDiscountDto? Discount);

    public record UpdateOrderDiscountDto(
        decimal Value,
        DiscountType Type,
        string? Reason);

    public record UpdateMoneyDto(decimal Amount, string Currency = "LKR");
}
