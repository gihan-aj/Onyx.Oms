using FluentValidation;
using Onyx.Oms.Core.Domain.Enums;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Orders.CreateOrder
{
    public record CreateOrderCommand(
        Guid CustomerId,
        bool IsCashOnDelivery,
        DateTimeOffset? OrderDate,
        List<OrderItemDto> Items,
        Guid? CourierId,
        ShippingAddressDto? ShippingAddress,
        MoneyDto? ShippingFee,
        MoneyDto? TaxAmount,
        OrderDiscountDto? Discount,
        InitialPaymentDto? Payment,
        string? Notes) : ICommand<Guid>;

    public record OrderItemDto(
        Guid ProductVariantId,
        int Quantity,
        OrderDiscountDto? Discount);

    public record OrderDiscountDto(
        decimal Value,
        DiscountType Type,
        string? Reason);

    public record ShippingAddressDto(
        string? Street,
        string? City,
        string? District,
        string? State,
        string? PostalCode,
        string? Country);

    public record InitialPaymentDto(
        MoneyDto Amount,
        PaymentMethod Method,
        string? Reference,
        DateTimeOffset PaymentDate);

    public record MoneyDto(decimal Amount, string Currency = "LKR");
}
